namespace Distributor_abstractions;

public interface IGeoProviderFactory
{
    /// <summary>
    /// Регистрация гео-провайдеров
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    void Register<T>(Func<T> factory)
        where T : IGeoProvider;

    /// <summary>
    /// Получение инстанса гео-провайдера
    /// </summary>
    /// <param name="providerType"></param>
    /// <returns>Инстанс гео-провайдера</returns>
    IGeoProvider Resolve(Type providerType);

    /// <summary>
    /// Получение инстанса гео-провайдера
    /// </summary>
    /// <param name="providerType"></param>
    /// <returns>Инстанс гео-провайдера</returns>
    IGeoProvider Resolve<T>() where T : IGeoProvider;
}

