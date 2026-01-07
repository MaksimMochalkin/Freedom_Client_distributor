using Distributor_domain;

namespace Distributor_abstractions;

public interface IAsyncPipelineStep<TInput, TOutput>
{
    /// <summary>
    /// Универсальный метод выполнения шага пайплайна
    /// </summary>
    /// <typeparam name="input">Контекст выполнения</typeparam>
    /// <param name="ct"></param>
    /// <returns>Result<TOutput></returns>
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct = default);
}
