using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.Dtos;

namespace Distributor_infrastructure.CsvStructValidator;

/// <summary>
/// Валидатор модели данных csv файлов
/// </summary>
public sealed class CsvSchemaValidator : ICsvSchemaValidator
{
    /// <summary>
    /// Метод валидации структуры источника данных
    /// </summary>
    /// <param name="dtoType"></param>
    /// <param name="header"></param>
    public Result<string> Validate(Type dtoType, string[] header)
    {
        Result<string> result = new Result<string>();
        if (dtoType == typeof(EmployeeDto))
            result = ValidateEmployee(header);
        else if (dtoType == typeof(BranchDto))
            result = ValidateBranch(header);
        else if (dtoType == typeof(ClientDto))
            result = ValidateClient(header);

        if (result.IsSuccess)
            return result;

        return Result<string>.Fail(new("VALIDATOR_NOT_FOUND", "Validator not found"));
    }

    private static Result<string> ValidateEmployee(string[] header)
    {
        var expected = new[]
        {
            "Сотрудник",
            "Филиал",
            "Число",
            "клиентов",
            "Навыки"
        };

        return ValidateExact(expected, header);
    }

    private static Result<string> ValidateBranch(string[] header)
    {
        var expected = new[]
        {
            "Филиал",
            "Широта",
            "Долгота",
            "Город",
        };

        return ValidateExact(expected, header);
    }

    private static Result<string> ValidateClient(string[] header)
    {
        var expected = new[]
        {
            "Клиент",
            "Страна",
            "Город",
            "Атрибуты"
        };

        return ValidateExact(expected, header);
    }

    private static Result<string> ValidateExact(string[] expected, string[] actual)
    {
        if (!expected.SequenceEqual(actual))
            return Result<string>.Fail(new("FILE_STRUCT_INVALID", $"File struct invalid: expected{expected}, actual: {actual}"));

        return Result<string>.Ok("Struct is valid");
    }
}
