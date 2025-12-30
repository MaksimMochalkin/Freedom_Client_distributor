using CsvHelper;

namespace Distributor_infrastructure.DtoMappers;

/// <summary>
/// Хэлпер для парсинга данных из csv файлов
/// </summary>
public static class CsvParserHelper
{
    public static string Build(string name, string number)
        => $"{name} {number}";

    public static string? GetFieldOrNull(this IReaderRow row, int index)
    {
        return row.Parser.Count > index
            ? row.GetField(index)
            : null;
    }
}
