using Distributor_abstractions;
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
    public void Validate(Type dtoType, string[] header)
    {
        if (dtoType == typeof(EmployeeDto))
            ValidateEmployee(header);
        else if (dtoType == typeof(BranchDto))
            ValidateBranch(header);
        else if (dtoType == typeof(ClientDto))
            ValidateClient(header);
    }

    private static void ValidateEmployee(string[] header)
    {
        var expected = new[]
        {
            "Сотрудник",
            "Филиал",
            "Число",
            "клиентов",
            "Навыки"
        };

        ValidateExact(expected, header);
    }

    private static void ValidateBranch(string[] header)
    {
        var expected = new[]
        {
            "Филиал",
            "Широта",
            "Долгота",
            "Город",
        };

        ValidateExact(expected, header);
    }

    private static void ValidateClient(string[] header)
    {
        var expected = new[]
        {
            "Клиент",
            "Страна",
            "Город",
            "Атрибуты"
        };

        ValidateExact(expected, header);
    }

    private static void ValidateExact(string[] expected, string[] actual)
    {
        if (!expected.SequenceEqual(actual))
            throw new CsvSchemaMismatchException(expected, actual);
    }
}

[Serializable]
internal class CsvSchemaMismatchException : Exception
{
    private string[] expected;
    private string[] actual;

    public CsvSchemaMismatchException()
    {
    }

    public CsvSchemaMismatchException(string? message) : base(message)
    {
    }

    public CsvSchemaMismatchException(string[] expected, string[] actual)
    {
        this.expected = expected;
        this.actual = actual;
    }

    public CsvSchemaMismatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}