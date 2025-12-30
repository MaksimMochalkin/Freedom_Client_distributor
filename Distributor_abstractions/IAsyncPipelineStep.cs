using Distributor_domain;

namespace Distributor_abstractions;

public interface IAsyncPipelineStep<TInput, TOutput>
{
    /// <summary>
    /// Универсальный метод выполнения шага пайплайна
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns>Result<TOutput></returns>
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct = default);
}
