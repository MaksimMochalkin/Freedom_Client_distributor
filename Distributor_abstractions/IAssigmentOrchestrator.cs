using Distributor_domain.CliStructs;

namespace Distributor_abstractions;

public interface IAssigmentOrchestrator
{
    /// <summary>
    /// Метод старта сбора и обработки данных
    /// </summary>
    /// <param name="loadOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Не возвращает данные. Записывает полученные данные в json файл</returns>
    Task RunAsync(
        CliOptions loadOptions,
        CancellationToken cancellationToken = default);
}
