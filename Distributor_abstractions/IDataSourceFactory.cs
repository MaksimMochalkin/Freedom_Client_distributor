namespace Distributor_abstractions;

public interface IDataSourceFactory
{
    /// <summary>
    /// Универсальный метод регистрации источника данных.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Конкретный инстанс источника данных. Принимает экземпляр наследника IDataSource</param>
    void Register<T>(IDataSource<T> source);
    /// <summary>
    /// Универсальный метод по получению инстанса конкретного источника данных. В качестве универсального параметра принимает DTO
    /// с которой работает источник данных
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="format">Формат с которым работает источник данных (csv, excel и т.д.)</param>
    /// <returns>Возвращает инстанс IDataSource</returns>
    IDataSource<T> Resolve<T>(string format);
}

