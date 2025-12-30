using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.CliStructs;
using Distributor_domain.Dtos;
using Distributor_infrastructure.ModelMappers;

namespace Destributor_services.DataReciverServices;

/// <summary>
/// Сервис получения данных по клиентам из csv файлов
/// </summary>
public class CsvDataResiverService : IDataResiverService<AssignmentContext>
{
    private readonly IDataSourceFactory _dataSourceFactory;

    public CsvDataResiverService(IDataSourceFactory dataSourceFactory)
    {
        _dataSourceFactory = dataSourceFactory;
    }

    /// <summary>
    /// Метод для загрузки данных по клиентам из источников
    /// </summary>
    /// <param name="loadOptions"></param>
    /// <returns></returns>
    public async Task<Result<AssignmentContext>> LoadAsync(CliOptions loadOptions, CancellationToken ct = default)
    {
        var employeeDataSource = _dataSourceFactory.Resolve<EmployeeDto>(loadOptions.Format);
        var branchDataSource = _dataSourceFactory.Resolve<BranchDto>(loadOptions.Format);
        var clientsDataSource = _dataSourceFactory.Resolve<ClientDto>(loadOptions.Format);

        var employeeDtosTask = employeeDataSource.LoadAsync(loadOptions.EmployeesPath, ct);
        var brancheDtosTask = branchDataSource.LoadAsync(loadOptions.BranchesPath, ct);
        var clientDtosTask = clientsDataSource.LoadAsync(loadOptions.ClientsPath, ct);

        await Task.WhenAll(employeeDtosTask, brancheDtosTask, clientDtosTask);
        // после выполнения WhenAll даже при повторном написании await реальной работы не происходит, происходит получение результата
        var employeeDtos = await employeeDtosTask;
        var brancheDtos = await brancheDtosTask;
        var clientDtos = await clientDtosTask;

        if (!employeeDtos.IsSuccess || !brancheDtos.IsSuccess || !clientDtos.IsSuccess)
            return Result<AssignmentContext>
                .Fail(new Error("CSV_LOAD_ERROR",
                string.Join(';', employeeDtos.Error?.Message,
                    brancheDtos.Error?.Message,
                    clientDtos.Error?.Message)));

        var clientsBusinesModel = clientDtos.Value!.Select(c => c.ToDomain()).ToList();
        var employeesBusinesModel = employeeDtos.Value!.Select(e => e.ToDomain()).ToList();
        var branchesBusinesModel = brancheDtos.Value!.Select(b => b.ToDomain()).ToList();
        var result = new AssignmentContext
        {
            Clients = clientsBusinesModel,
            Employees = employeesBusinesModel,
            Branches = branchesBusinesModel,
        };
        return Result<AssignmentContext>.Ok(result);
    }
}
