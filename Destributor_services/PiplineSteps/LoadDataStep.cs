using Distributor_abstractions;
using Distributor_domain;

namespace Destributor_services.Staps;

/// <summary>
/// Класс загрузки данных из файлов
/// </summary>
public sealed class LoadDataStep
    : IAsyncPipelineStep<AssignmentContext, AssignmentContext>
{
    private readonly IDataResiverService<AssignmentContext> _dataReceiver;

    public LoadDataStep(IDataResiverService<AssignmentContext> dataReceiver)
    {
        _dataReceiver = dataReceiver;
    }

    /// <summary>
    /// Универсальный метод выполнения шага пайплайна
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns>Result<AssignmentContext></returns>
    public async Task<Result<AssignmentContext>> ExecuteAsync(AssignmentContext input, CancellationToken ct = default)
    {
        return await _dataReceiver.LoadAsync(input.CliOptions!, ct);
    }
}
