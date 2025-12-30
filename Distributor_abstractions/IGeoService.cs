namespace Distributor_abstractions;

public interface IGeoService
{
    /// <summary>
    /// Метод получения гео-позиции клиента
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает гео-позицию клиента</returns>
    Task<(double lat, double lon)?> GetCoordinatesAsync<TProvider>(
        string country,
        string city,
        CancellationToken token = default) where TProvider : IGeoProvider;
}
