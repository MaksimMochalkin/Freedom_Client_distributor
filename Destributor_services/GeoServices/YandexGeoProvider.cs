using Distributor_abstractions;
using System.Globalization;
using System.Text.Json;

namespace Destributor_services.GeoServices;

/// <summary>
/// Получение реальных данных о гео-позиции клиента
/// </summary>
public sealed class YandexGeoProvider : IGeoProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public YandexGeoProvider(HttpClient http, string apiKey)
    {
        _http = http;
        _apiKey = apiKey;
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
        var query = $"{country},{city}";
        var url =
            $"https://geocode-maps.yandex.ru/v1/" +
            $"?apikey={_apiKey}&format=json&geocode={Uri.EscapeDataString(query)}";

        var response = await _http.GetStringAsync(url, token);

        using var doc = JsonDocument.Parse(response);

        var pos = doc
            .RootElement
            .GetProperty("response")
            .GetProperty("GeoObjectCollection")
            .GetProperty("featureMember")
            .EnumerateArray()
            .FirstOrDefault()
            .GetProperty("GeoObject")
            .GetProperty("Point")
            .GetProperty("pos")
            .GetString();

        if (pos is null)
            return null;

        var parts = pos.Split(' ');
        return (
            double.Parse(parts[1], CultureInfo.InvariantCulture),
            double.Parse(parts[0], CultureInfo.InvariantCulture)
        );
    }
}
