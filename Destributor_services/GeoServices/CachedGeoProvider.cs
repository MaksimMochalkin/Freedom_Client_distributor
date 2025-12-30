using Distributor_abstractions;
using System.Collections.Concurrent;

namespace Destributor_services.GeoServices;

/// <summary>
/// Гео-провайдер с кешем
/// </summary>
public sealed class CachedGeoProvider : IGeoProvider
{
    private readonly IGeoProvider _inner;
    private readonly ConcurrentDictionary<string, (double, double)?> _cache = new();

    public CachedGeoProvider(IGeoProvider inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Метод получения гео-позиции клиента
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает гео-позицию клиента</returns>
    public async Task<(double lat, double lon)?> GetCoordinatesAsync(string country, string city, CancellationToken token = default)
    {
        var key = $"{country}|{city}".ToLowerInvariant();

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var value = await _inner.GetCoordinatesAsync(country, city, token);
        _cache[key] = value;

        return value;
    }
}
