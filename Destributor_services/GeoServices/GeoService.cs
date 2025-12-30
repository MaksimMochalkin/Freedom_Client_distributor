using Distributor_abstractions;

namespace Destributor_services.GeoServices;

/// <summary>
/// Реализация сервиса получения гео-данных клиента
/// </summary>
public class GeoService : IGeoService
{
    private readonly IGeoProviderFactory _geoProviderFactory;

    public GeoService(IGeoProviderFactory geoProviderFactory)
    {
        _geoProviderFactory = geoProviderFactory;
    }

    /// <summary>
    /// Метод получения гео-позиции клиента
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает гео-позицию клиента</returns>
    public Task<(double lat, double lon)?> GetCoordinatesAsync<TProvider>(
        string country,
        string city,
        CancellationToken token = default)
        where TProvider : IGeoProvider
    {
        var provider = _geoProviderFactory.Resolve<TProvider>();
        return provider.GetCoordinatesAsync(country, city, token);
    }
}
