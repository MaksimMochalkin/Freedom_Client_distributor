using CsvHelper.Configuration;
using Distributor_domain.Dtos;
using System.Text.Json;

namespace Distributor_infrastructure.DtoMappers;

/// <summary>
/// Маппер для клиентов
/// </summary>
public sealed class ClientDtoMapper : ClassMap<ClientDto>
{
    public ClientDtoMapper()
    {
        // 0: Клиент
        Map(m => m.ClientId).Index(0);

        // 1: Страна
        Map(m => m.Country).Convert(args =>
        {
            var value = args.Row.GetFieldOrNull(1);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        });

        // 2: Город
        Map(m => m.City).Convert(args =>
        {
            var row = args.Row;
            var fields = GetAllFields(row);

            var attrIndex = FindAttributesIndex(fields);
            if (attrIndex < 0)
                return null;

            if (attrIndex <= 2)
                return row.GetFieldOrNull(2);

            return string.Join(" ",
                fields.Skip(2).Take(attrIndex - 2)
            );
        });

        // 3: Атрибуты
        Map(m => m.Attributes).Convert(args =>
        {
            var fields = GetAllFields(args.Row);
            var attrIndex = FindAttributesIndex(fields);

            if (attrIndex < 0)
                return Array.Empty<string>();

            var raw = fields[attrIndex];

            if (string.IsNullOrWhiteSpace(raw) || raw == "[]")
                return Array.Empty<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(raw)
                       ?? new List<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        });
    }

    private static string[] GetAllFields(CsvHelper.IReaderRow row)
    {
        var count = row.Parser.Count;
        var fields = new string[count];

        for (var i = 0; i < count; i++)
            fields[i] = row.GetField(i);

        return fields;
    }

    private static int FindAttributesIndex(string[] fields)
    {
        for (var i = fields.Length - 1; i >= 0; i--)
        {
            var value = fields[i];

            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (value.Trim() == "[]")
                return i;

            try
            {
                JsonSerializer.Deserialize<List<string>>(value);
                return i;
            }
            catch
            {
            }
        }

        return -1;
    }
}
