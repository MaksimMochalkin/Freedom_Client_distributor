namespace Distributor_abstractions;

public interface IGeoProvider
{
    /// <summary>
    /// Метод получения гео-позиции клиента
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает гео-позицию клиента</returns>
    Task<(double lat, double lon)?> GetCoordinatesAsync(string country, string city, CancellationToken token = default);
}
