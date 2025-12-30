using CsvHelper.Configuration;
using Distributor_domain.Dtos;

namespace Distributor_infrastructure.DtoMappers;

/// <summary>
/// Маппер для филиалов
/// </summary>
public sealed class BranchDtoMapper : ClassMap<BranchDto>
{
    public BranchDtoMapper()
    {
        // "Отдел" + " " + "1" => "Отдел 1"
        Map(m => m.Branch).Convert(args =>
        {
            var name = args.Row.GetField(0);
            var number = args.Row.GetField(1);
            return CsvParserHelper.Build(name!, number!);
        });

        Map(m => m.Latitude).Index(2);
        Map(m => m.Longitude).Index(3);
        Map(m => m.City).Index(4);
    }
}
