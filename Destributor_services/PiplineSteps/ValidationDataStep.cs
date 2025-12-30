using Distributor_abstractions;
using Distributor_domain;

namespace Destributor_services.PiplineSteps;

public sealed class ValidationDataStep
    : IAsyncPipelineStep<AssignmentContext, AssignmentContext>
{
    public ValidationDataStep() { }

    public async Task<Result<AssignmentContext>> ExecuteAsync(AssignmentContext input, CancellationToken ct = default)
    {
        //здесь должна быть логика по валидации полученных данных из источников согласно допустимой бизнес логике
        //но данная реализация выходит за рамки текущего ТЗ. Класс создан чтобы показать, что после считывания данные должны быть провалидированы
        return await Task.FromResult(Result<AssignmentContext>.Ok(input));
    }
}
