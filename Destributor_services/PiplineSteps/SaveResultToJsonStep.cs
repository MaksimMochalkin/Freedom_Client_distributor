using Distributor_abstractions;
using Distributor_domain;
using System.Text.Json;

namespace Destributor_services.Staps;

/// <summary>
/// Шаг записи результата в json
/// </summary>
public sealed class SaveResultToJsonStep
    : IAsyncPipelineStep<AssignmentContext, AssignmentContext>
{
    private readonly string _filePath;

    public SaveResultToJsonStep(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Универсальный метод выполнения шага пайплайна
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns>Result<AssignmentContext></returns>
    public async Task<Result<AssignmentContext>> ExecuteAsync(AssignmentContext input, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(
            new { data = input.Assignments },
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, json, ct);

        return Result<AssignmentContext>.Ok(input);
    }
}
