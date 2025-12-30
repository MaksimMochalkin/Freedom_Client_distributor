namespace Distributor_infrastructure.DtoMappers;

using CsvHelper.Configuration;
using Distributor_domain.Dtos;
using System.Text.Json;

/// <summary>
/// Маппер данных сотрудников
/// </summary>
public sealed class EmployeeDtoMapper : ClassMap<EmployeeDto>
{
    public EmployeeDtoMapper()
    {
        // 0: "Сотрудник"
        Map(m => m.EmployeeId).Index(0);

        // 1: "Филиал", 2: "Число" => "Отдел 1"
        Map(m => m.Branch).Convert(args =>
        {
            var branch = args.Row.GetField(1);
            var number = args.Row.GetField(2);
            return CsvParserHelper.Build(branch!, number!);
        });

        // 3: "клиентов"
        Map(m => m.ClientCount).Index(3);

        // 4: "Навыки"
        Map(m => m.Skills).Convert(args =>
        {
            var raw = args.Row.GetField(4);
            return JsonSerializer.Deserialize<List<string>>(raw!)!;
        });
    }
}
