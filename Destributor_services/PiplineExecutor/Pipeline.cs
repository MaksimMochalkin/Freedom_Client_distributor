using Distributor_abstractions;
using Distributor_domain;

namespace Destributor_services.PiplineExecutor;

/// <summary>
/// Ядро выполнения паййплайна по получению и обработке данных
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Pipeline<T>
{
    private readonly IReadOnlyList<IAsyncPipelineStep<T, T>> _steps;

    public Pipeline(IEnumerable<IAsyncPipelineStep<T, T>> steps)
    {
        _steps = steps.ToList();
    }

    /// <summary>
    /// Обработка шагов пайплайна
    /// </summary>
    /// <typeparam name="input">Контекст выполнения</typeparam>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Result<T></returns>
    public async Task<Result<T>> ExecuteAsync(T input, CancellationToken ct = default)
    {
        var current = Result<T>.Ok(input);

        foreach (var step in _steps)
        {
            if (!current.IsSuccess)
                return current;

            current = await step.ExecuteAsync(current.Value!, ct);
        }

        return current;
    }
}
