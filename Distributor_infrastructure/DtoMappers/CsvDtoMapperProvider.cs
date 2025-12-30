using CsvHelper.Configuration;
using Distributor_abstractions;
using Distributor_domain.Dtos;

namespace Distributor_infrastructure.DtoMappers;

/// <summary>
/// Фабрика мапперов данных на dto
/// </summary>
public class CsvDtoMapperProvider : ICsvDtoMapperProvider
{
    /// <summary>
    /// Метод получения маппера для парсинга данных на основе dto модели
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Конкретный инстанс маппера</returns>
    public ClassMap GetMap(Type type) =>
    type switch
    {
        var t when t == typeof(EmployeeDto) => new EmployeeDtoMapper(),
        var t when t == typeof(BranchDto) => new BranchDtoMapper(),
        var t when t == typeof(ClientDto) => new ClientDtoMapper(),
        _ => throw new InvalidOperationException($"No CSV map for {type.Name}")
    };
}
