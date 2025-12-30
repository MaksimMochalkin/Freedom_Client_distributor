using Distributor_abstractions;
using Distributor_domain;

namespace Destributor_services.Staps;

/// <summary>
/// Назначение клиентов на менеджеров
/// </summary>
public sealed class ClientAssignmentStep
    : IAsyncPipelineStep<AssignmentContext, AssignmentContext>
{
    private readonly IClientAssignmentService _assignmentService;

    public ClientAssignmentStep(IClientAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    public Result<AssignmentContext> Execute(AssignmentContext input)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Универсальный метод выполнения шага пайплайна
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ct"></param>
    /// <returns>Result<AssignmentContext></returns>
    public async Task<Result<AssignmentContext>> ExecuteAsync(
        AssignmentContext context,
        CancellationToken ct = default)
    {
        var result = await _assignmentService.AssignAsync(
            context.Clients,
            context.Employees,
            context.Branches,
            ct);

        if (!result.IsSuccess)
            return Result<AssignmentContext>.Fail(result.Error!);

        return Result<AssignmentContext>.Ok(
            context with { Assignments = result.Value });
    }
}
