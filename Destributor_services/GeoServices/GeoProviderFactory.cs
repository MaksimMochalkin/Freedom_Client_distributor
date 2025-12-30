using Distributor_abstractions;

namespace Destributor_services.GeoServices;

/// <summary>
/// Фабрика гео-провайдеров
/// </summary>
public sealed class GeoProviderFactory : IGeoProviderFactory
{
    private readonly Dictionary<Type, Func<IGeoProvider>> _factories = new();

    /// <summary>
    /// Регистрация гео-провайдеров
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    public void Register<T>(Func<T> factory)
        where T : IGeoProvider
    {
        _factories[typeof(T)] = () => factory();
    }

    /// <summary>
    /// Получение инстанса гео-провайдера
    /// </summary>
    /// <param name="providerType"></param>
    /// <returns>Инстанс гео-провайдера</returns>
    public IGeoProvider Resolve<T>() where T : IGeoProvider
        => Resolve(typeof(T));

    /// <summary>
    /// Получение инстанса гео-провайдера
    /// </summary>
    /// <param name="providerType"></param>
    /// <returns>Инстанс гео-провайдера</returns>
    public IGeoProvider Resolve(Type providerType)
    {
        if (!_factories.TryGetValue(providerType, out var factory))
            throw new InvalidOperationException(
                $"Geo provider '{providerType.Name}' is not registered");

        // 👉 стратегия
        var provider = factory();

        // 👉 декоратор
        return new CachedGeoProvider(provider);
    }
}
