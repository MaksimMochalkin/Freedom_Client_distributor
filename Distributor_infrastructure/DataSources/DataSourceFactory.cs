using Distributor_abstractions;

namespace Distributor_infrastructure.DataSources;

/// <summary>
/// Фабрика источников данных
/// </summary>
public sealed class DataSourceFactory : IDataSourceFactory
{
    private readonly Dictionary<(Type, string), object> _sources = new();

    /// <summary>
    /// Универсальный метод регистрации источника данных. В качестве источника данных принимает экземпляр наследника IDataSource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Конкретный инстанс источника данных. Принимает экземпляр наследника IDataSource</param>
    public void Register<T>(IDataSource<T> source)
        => _sources[(typeof(T), source.Format)] = source;

    /// <summary>
    /// Универсальный метод по получению инстанса конкретного источника данных. В качестве универсального параметра принимает DTO
    /// с которой работает источник данных
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="format">Формат с которым работает источник данных (csv, excel и т.д.)</param>
    /// <returns>Возвращает инстанс IDataSource</returns>
    public IDataSource<T> Resolve<T>(string format)
        => (IDataSource<T>)_sources[(typeof(T), format)];
}

