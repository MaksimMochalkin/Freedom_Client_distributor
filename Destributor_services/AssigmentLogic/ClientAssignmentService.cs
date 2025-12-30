using Destributor_services.DistanceCalc;
using Destributor_services.GeoServices;
using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Destributor_services.AssigmentLogic;

/// <summary>
/// Сервис по назначению клиентов на ближайший филиал и менеджеров
/// </summary>
public class ClientAssignmentService : IClientAssignmentService
{
    private readonly IGeoService _geoService;
    private readonly Channel<Client> _vipChannel = Channel.CreateBounded<Client>(1024);
    private readonly Channel<Client> _geoChannel = Channel.CreateBounded<Client>(4096);
    private readonly Channel<Client> _fallbackChannel = Channel.CreateBounded<Client>(1024);
    private readonly Channel<AssignmentResult> _resultChannel = Channel.CreateUnbounded<AssignmentResult>();

    public ClientAssignmentService(IGeoService geoService)
    {
        _geoService = geoService;
    }

    /// <summary>
    /// Метод назначения клиентов на ближайший филиал и наименее загруженного менеджера
    /// </summary>
    /// <param name="clients"></param>
    /// <param name="employees"></param>
    /// <param name="branches"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Result<IReadOnlyList<AssignmentResult>>> AssignAsync(
        IReadOnlyList<Client> clients,
        IReadOnlyList<Employee> employees,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        // 1. Маршрутизация в отдельные очереди по типу клиентов
        foreach (var client in clients)
        {
            if (client.IsVip)
                await _vipChannel.Writer.WriteAsync(client, token);
            else if (!string.Equals(client.Country, "Казахстан", StringComparison.OrdinalIgnoreCase))
                await _fallbackChannel.Writer.WriteAsync(client, token);
            else
                await _geoChannel.Writer.WriteAsync(client, token);
        }

        _vipChannel.Writer.Complete();
        _geoChannel.Writer.Complete();

        Task Run(ValueTask vt) => vt.AsTask();
        
        await Task.WhenAll(
            Run(ProcessVipAsync(employees, token)),
            Run(ProcessGeoAsync(employees, branches, token))
        );

        _fallbackChannel.Writer.Complete();
        await Run(ProcessFallbackAsync(employees, branches, token));
        _resultChannel.Writer.Complete();

        var collectTask = CollectResultsAsync(token);
        var result = await collectTask;
        return Result<IReadOnlyList<AssignmentResult>>.Ok(result);
    }

    /// <summary>
    /// Метод обработки клиентов с приставкой VIP. Для клиентов данного класса не имеет значения где они находятся,
    /// они назначаются на наименее загруженного менеджера с возможностью работать с VIP клиентами
    /// </summary>
    /// <param name="employees"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async ValueTask ProcessVipAsync(
        IReadOnlyList<Employee> employees,
        CancellationToken token = default)
    {
        await foreach (var client in _vipChannel.Reader.ReadAllAsync(token))
        {
            var employee = SelectLeastLoaded(
                employees.Where(e => e.HasSkill("VIP")));

            var workload = employee.AssignClient();

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    /// <summary>
    /// Метод по обработке клиентов которые не имеют статуса VIP. Данный метод назначает клиентов на менеджеров исходя из местоположения клиента
    /// выбирая филиал и менеджера который находится к ним ближе всего
    /// </summary>
    /// <param name="employees"></param>
    /// <param name="branches"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async ValueTask ProcessGeoAsync(
        IReadOnlyList<Employee> employees,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        await foreach (var client in _geoChannel.Reader.ReadAllAsync(token))
        {
            var clientCoords = await _geoService.GetCoordinatesAsync<MockGeoProvider>(
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
                var dictance = DistanceCalculator.Distance(
                    clientCoords.Value.lat, clientCoords.Value.lon,
                    branch.Latitude, branch.Longitude);

                if (dictance < minDistance)
                {
                    minDistance = dictance;
                    nearest = branch;
                }
            }

            var employee = SelectLeastLoaded(
                employees.Where(e => e.Branch == nearest!.Name));

            var workload = employee.AssignClient();

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    /// <summary>
    /// Метод по обработке клиентов не из Казахстана и клиентов для которых не удалось определить гео-позицию. Такие клиенты назначаются на
    /// менеджеров филиалов Алматы и Астаны
    /// </summary>
    /// <param name="employees"></param>
    /// <param name="branches"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async ValueTask ProcessFallbackAsync(
        IReadOnlyList<Employee> employees,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default)
    {
        var fallbackBranches = branches
            .Where(b => b.City is "Алматы" or "Астана")
            .Select(b => b.Name)
            .ToHashSet();

        await foreach (var client in _fallbackChannel.Reader.ReadAllAsync(token))
        {
            var employee = SelectLeastLoaded(
                employees.Where(e => fallbackBranches.Contains(e.Branch)));

            var workload = employee.AssignClient();

            await _resultChannel.Writer.WriteAsync(
                new AssignmentResult(employee.Id, client.Id, workload),
                token);
        }
    }

    /// <summary>
    /// Метод формирующий финальный спиок клиентов и менеджеров на которых они были назначены
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<IReadOnlyList<AssignmentResult>> CollectResultsAsync(
        CancellationToken token = default)
    {
        var list = new List<AssignmentResult>();

        await foreach (var item in _resultChannel.Reader.ReadAllAsync(token))
            list.Add(item);

        return list;
    }

    /// <summary>
    /// Метод по определению наименее загруженного менеджера в данный момент
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    private static Employee SelectLeastLoaded(IEnumerable<Employee> employees)
    {
        Employee? selected = null;
        var min = int.MaxValue;

        foreach (var e in employees)
        {
            var load = e.TotalWorkload;
            if (load < min)
            {
                min = load;
                selected = e;
            }
        }

        return selected!;
    }
}
