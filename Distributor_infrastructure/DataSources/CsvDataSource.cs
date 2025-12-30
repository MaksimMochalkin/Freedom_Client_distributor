using CsvHelper;
using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.LogicErrors;
using System.Globalization;

namespace Distributor_infrastructure.DataSources;

/// <summary>
/// Парсер данных из csv
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class CsvDataSource<T> : IDataSource<T>
{
    /// <summary>
    /// Формат источника данных
    /// </summary>
    public string Format => "csv";
    private readonly ICsvDtoMapperProvider _mapProvider;
    private readonly ICsvSchemaValidator _schemaValidator;

    public CsvDataSource(ICsvDtoMapperProvider mapProvider,
        ICsvSchemaValidator schemaValidator)
    {
        _mapProvider = mapProvider;
        _schemaValidator = schemaValidator;
    }

    /// <summary>
    /// Метод по загрузке данных из источника
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Возвращает список dto моделями данных</returns>
    public async Task<Result<IReadOnlyList<T>>> LoadAsync(string path, CancellationToken ct = default)
    {
        try
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await csv.ReadAsync();
            csv.ReadHeader();

            var header = csv.Context.Reader!.HeaderRecord!;
            _schemaValidator.Validate(typeof(T), header);

            var map = _mapProvider.GetMap(typeof(T));
            csv.Context.RegisterClassMap(map);

            var records = new List<T>();

            await foreach (var record in csv.GetRecordsAsync<T>(ct))
            {
                records.Add(record);
            }
            return Result<IReadOnlyList<T>>.Ok(records);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return Result<IReadOnlyList<T>>.Fail(LogicError.CsvReaderError);
        }
    }
}
