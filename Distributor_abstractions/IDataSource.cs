using Distributor_domain;

namespace Distributor_abstractions;

public interface IDataSource<T>
{
    /// <summary>
    /// Формат источника данных
    /// </summary>
    string Format { get; }
    /// <summary>
    /// Метод по загрузке данных из источника
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Возвращает список dto моделями данных</returns>
    Task<Result<IReadOnlyList<T>>> LoadAsync(string path, CancellationToken ct = default);
}
