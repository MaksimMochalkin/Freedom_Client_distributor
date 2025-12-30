using Destributor_services.PiplineExecutor;
using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.CliStructs;
namespace Client_distributor;

/// <summary>
/// Оркестратор сбора и обработки данных
/// </summary>
public sealed class AssigmentOrchestrator : IAssigmentOrchestrator
{
    private readonly Pipeline<AssignmentContext> _pipeline;

    public AssigmentOrchestrator(
        Pipeline<AssignmentContext> pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// Метод старта сбора и обработки данных
    /// </summary>
    /// <param name="loadOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Не возвращает данные. Записывает полученные данные в json файл</returns>
    public async Task RunAsync(
        CliOptions loadOptions,
        CancellationToken cancellationToken = default)
    {
        var context = new AssignmentContext { CliOptions = loadOptions };
        var result = await _pipeline.ExecuteAsync(
            context,
            cancellationToken);

        if (!result.IsSuccess)
            throw new Exception(result.Error!.Message);
    }
}
