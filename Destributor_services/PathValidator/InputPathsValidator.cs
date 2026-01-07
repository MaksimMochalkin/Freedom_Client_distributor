using Distributor_domain;
using Distributor_domain.CliStructs;

namespace Destributor_services.PathValidator;

/// <summary>
/// Валидатор входных параметров CLI, отвечающий за проверку путей к входным файлам.
/// </summary>
public static class InputPathsValidator
{
    /// <summary>
    /// Выполняет валидацию всех входных путей, переданных через CLI-параметры.
    ///
    /// В случае первой ошибки валидации метод немедленно возвращает
    /// <see cref="Result{T}.Fail"/>, содержащий описание ошибки.
    /// </summary>
    /// <param name="options">
    /// Набор CLI-параметров, содержащий пути к входным файлам и ожидаемый формат файлов.
    /// </param>
    /// <returns>
    /// <see cref="Result{T}"/> с:
    /// <list type="bullet">
    /// <item><description><c>Ok(options)</c>, если все пути валидны</description></item>
    /// <item><description><c>Fail(error)</c>, если хотя бы один путь не прошёл проверку</description></item>
    /// </list>
    /// </returns>
    public static Result<CliOptions> ValidateInputParams(CliOptions options)
    {
        var validations = new[]
        {
            (options.EmployeesPath, nameof(options.EmployeesPath)),
            (options.BranchesPath, nameof(options.BranchesPath)),
            (options.ClientsPath, nameof(options.ClientsPath))
        };

        foreach (var (path, name) in validations)
        {
            var result = ValidateInputPath(path, options.Format, name);
            if (!result.IsSuccess)
                return Result<CliOptions>.Fail(result.Error!);
        }

        return Result<CliOptions>.Ok(options);
    }

    private static Result<string> ValidateInputPath(
        string path,
        string expectedFormat,
        string argumentName)
    {
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return Result<string>.Fail(
                CliErrors.InvalidInputParam($"{argumentName} contains invalid characters."));

        var fullPath = Path.GetFullPath(path);

        if (Directory.Exists(fullPath))
            return Result<string>.Fail(
                CliErrors.InvalidInputParam($"{argumentName} is a directory, not a file."));

        if (!File.Exists(fullPath))
            return Result<string>.Fail(
                CliErrors.InvalidInputParam($"{argumentName} file not found: {fullPath}"));
            
        var extension = Path.GetExtension(fullPath)
            .TrimStart('.')
            .ToLowerInvariant();

        if (!extension.Equals(expectedFormat, StringComparison.OrdinalIgnoreCase))
            return Result<string>.Fail(
                CliErrors.InvalidInputParam($"{argumentName} has invalid extension '.{extension}'. Expected '.{expectedFormat}'."));
        try
        {
            using var stream = File.OpenRead(fullPath);
        }
        catch (UnauthorizedAccessException)
        {
            return Result<string>.Fail(
                CliErrors.InvalidInputParam($"{argumentName} access denied: {fullPath}"));
        }

        return Result<string>.Ok("Correct param");
    }
}
