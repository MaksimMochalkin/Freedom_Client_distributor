using Distributor_abstractions;

namespace Destributor_services.GeoServices;

/// <summary>
/// Моковые данные по гео-позиции клиента
/// </summary>
public class MockGeoProvider : IGeoProvider
{
    private static readonly Dictionary<string, (double, double)> CityMap = new()
    {
        ["Алматы"] = (43.23716, 76.94565),
        ["Астана"] = (51.1282, 71.43043),
        ["Костанай"] = (53.2145, 63.6321),
        ["Шымкент"] = (42.31552, 69.58694),
        ["Караганда"] = (49.80776, 73.0885),
        ["Павлодар"] = (52.28558, 76.94072),
        ["Актау"] = (43.63559, 51.16824),
        ["Бишкек"] = (42.87597, 74.6037)
    };

    /// <summary>
    /// Метод получения гео-позиции клиента
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает гео-позицию клиента</returns>
    public async Task<(double lat, double lon)?> GetCoordinatesAsync(string country, string city, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            return null;

        return CityMap.TryGetValue(city.Trim(), out var geo)
            ? await Task.FromResult(geo) : null;
    }
}
