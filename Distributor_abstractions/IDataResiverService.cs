using Distributor_domain;
using Distributor_domain.CliStructs;

namespace Distributor_abstractions;

public interface IDataResiverService<TResult>
{
    /// <summary>
    /// Метод для загрузки данных по клиентам из источников
    /// </summary>
    /// <param name="loadOptions"></param>
    /// <returns>Возвращает обработанные данные по клиентам, филиалам и менеджерам</returns>
    Task<Result<TResult>> LoadAsync(CliOptions loadOptions, CancellationToken ct = default);
}
