using Destributor_services.DistanceCalc;
using Destributor_services.GeoServices;
using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.Models;
using System.Threading.Channels;

namespace Destributor_services.AssigmentLogic;

public class ClientAssignmentService : IClientAssignmentService
{
    private readonly IGeoService _geoService;

    private readonly Channel<Client> _vipChannel = Channel.CreateBounded<Client>(
        new BoundedChannelOptions(1024) { SingleReader = false, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });

    private readonly Channel<Client> _geoChannel = Channel.CreateBounded<Client>(
        new BoundedChannelOptions(4096) { SingleReader = false, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });

    private readonly Channel<Client> _fallbackChannel = Channel.CreateBounded<Client>(
        new BoundedChannelOptions(1024) { SingleReader = false, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });

    private readonly Channel<AssignmentResult> _resultChannel = Channel.CreateBounded<AssignmentResult>(
        new BoundedChannelOptions(8192) { SingleReader = true, SingleWriter = false, FullMode = BoundedChannelFullMode.Wait });

    public ClientAssignmentService(IGeoService geoService)
    {
        _geoService = geoService;
    }

    public async Task<Result<IReadOnlyList<AssignmentResult>>> AssignAsync(
        IReadOnlyList<Client> clients,
        IReadOnlyList<Employee> employees,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        // Precompute lookups to avoid repeated LINQ allocations in hot path
        var employeesByBranch = employees.GroupBy(e => e.Branch)
                                         .ToDictionary(g => g.Key, g => g.ToArray());

        var vipEmployees = employees.Where(e => e.HasSkill("VIP")).ToArray();

        // Start collector immediately so results are consumed as produced
        var collectTask = CollectResultsAsync(token);

        // 1. Routing into channels
        foreach (var client in clients)
        {
            token.ThrowIfCancellationRequested();

            if (client.IsVip)
                await _vipChannel.Writer.WriteAsync(client, token);
            else if (!string.Equals(client.Country, "Казахстан", StringComparison.OrdinalIgnoreCase))
                await _fallbackChannel.Writer.WriteAsync(client, token);
            else
                await _geoChannel.Writer.WriteAsync(client, token);
        }

        // No more writes to vip and geo channels
        _vipChannel.Writer.Complete();
        _geoChannel.Writer.Complete();

        // Determine workers count sensibly
        var cpu = Math.Max(1, Environment.ProcessorCount);
        var vipWorkers = Math.Min(4, cpu);
        var geoWorkers = Math.Min(cpu * 2, cpu * 2); // geo may be I/O bound
        var fallbackWorkers = Math.Min(2, cpu);

        // Start workers
        var vipTasks = Enumerable.Range(0, vipWorkers)
            .Select(_ => ProcessVipWorkerAsync(vipEmployees, token)).ToArray();

        var geoTasks = Enumerable.Range(0, geoWorkers)
            .Select(_ => ProcessGeoWorkerAsync(employeesByBranch, branches, token)).ToArray();

        // Wait vip + geo workers
        await Task.WhenAll(vipTasks.Concat(geoTasks));

        // Now no more writes to fallback channel from geo stage
        _fallbackChannel.Writer.Complete();

        // Start fallback workers and wait
        var fallbackTasks = Enumerable.Range(0, fallbackWorkers)
            .Select(_ => ProcessFallbackAsync(employeesByBranch, branches, token)).ToArray();

        await Task.WhenAll(fallbackTasks);

        // All producers done → complete results and wait collection
        _resultChannel.Writer.Complete();

        var result = await collectTask;
        return Result<IReadOnlyList<AssignmentResult>>.Ok(result);
    }

    // Worker wrappers call the original logic but avoid repeating LINQ allocations on every item.
    private async Task ProcessVipWorkerAsync(Employee[] vipEmployees, CancellationToken token = default)
    {
        await foreach (var client in _vipChannel.Reader.ReadAllAsync(token))
        {
            var (employee, workload) = SelectLeastLoaded(vipEmployees);

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    private async Task ProcessGeoWorkerAsync(
        Dictionary<string, Employee[]> employeesByBranch,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        await foreach (var client in _geoChannel.Reader.ReadAllAsync(token))
        {
            var clientCoords = await _geoService.GetCoordinatesAsync<YandexGeoProvider>(
                client.Country!, client.City!, token);

            if (clientCoords is null)
            {
                await _fallbackChannel.Writer.WriteAsync(client, token);
                continue;
            }

            Branch? nearest = null;
            var minDistance = double.MaxValue;

            foreach (var branch in branches)
            {
                var distance = DistanceCalculator.Distance(
                    clientCoords.Value.lat, clientCoords.Value.lon,
                    branch.Latitude, branch.Longitude);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = branch;
                }
            }

            if (nearest is null)
            {
                await _fallbackChannel.Writer.WriteAsync(client, token);
                continue;
            }

            if (!employeesByBranch.TryGetValue(nearest.Name, out var branchEmployees) || branchEmployees.Length == 0)
            {
                await _fallbackChannel.Writer.WriteAsync(client, token);
                continue;
            }

            var (employee, workload) = SelectLeastLoaded(branchEmployees);

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    private async Task ProcessFallbackAsync(
        Dictionary<string, Employee[]> employeesByBranch,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        var fallbackBranches = branches
            .Where(b => b.City is "Алматы" or "Астана")
            .Select(b => b.Name)
            .ToHashSet();

        await foreach (var client in _fallbackChannel.Reader.ReadAllAsync(token))
        {
            // gather employees for fallback branches in place
            var candidates = fallbackBranches.SelectMany(b =>
                employeesByBranch.TryGetValue(b, out var arr) ? arr : Array.Empty<Employee>()).ToArray();

            var (employee, workload) = SelectLeastLoaded(candidates);

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    private async Task<IReadOnlyList<AssignmentResult>> CollectResultsAsync(
        CancellationToken token = default)
    {
        var list = new List<AssignmentResult>();

        await foreach (var item in _resultChannel.Reader.ReadAllAsync(token))
            list.Add(item);

        return list;
    }

    private static (Employee employee, int workload) SelectLeastLoaded(IEnumerable<Employee> employees)
    {
        var spinner = new SpinWait();

        while (true)
        {
            Employee? best = null;
            var bestLoad = int.MaxValue;

            // 1. Снимаем snapshot нагрузок
            foreach (var e in employees)
            {
                var load = e.TotalWorkload;
                if (load < bestLoad)
                {
                    bestLoad = load;
                    best = e;
                }
            }

            if (best is null)
                throw new InvalidOperationException("No employees available");

            // 2. Пытаемся зарезервировать кандидата
            if (!best.TryReserve())
            {
                spinner.SpinOnce();
                continue;
            }

            try
            {
                // 3. Перепроверяем: не появился ли лучший кандидат
                var stillBest = true;

                foreach (var e in employees)
                {
                    if (ReferenceEquals(e, best))
                        continue;

                    if (e.TotalWorkload < best.TotalWorkload)
                    {
                        stillBest = false;
                        break;
                    }
                }

                if (!stillBest)
                {
                    // уступаем более свободному сотруднику
                    continue;
                }

                // 4. Назначаем клиента
                var workload = best.AssignClient();
                return (best, workload);
            }
            finally
            {
                best.ReleaseReservation();
            }
        }
    }
}