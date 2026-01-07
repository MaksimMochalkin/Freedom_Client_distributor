using Distributor_domain;
using Distributor_domain.CliStructs;

namespace Client_distributor;

/// <summary>
/// Класс парсинга входящих параметров от пользователя
/// </summary>
public static class CliStartParserService
{
    /// <summary>
    /// Метод обработки путей к файлам
    /// </summary>
    /// <param name="args"></param>
    /// <returns>
    /// <see cref="Result{T}"/> с:
    /// <list type="bullet">
    /// <item><description><c>Ok(options)</c>, если все пути валидны</description></item>
    /// <item><description><c>Fail(error)</c>, если хотя бы один аргумент не прошёл проверку</description></item>
    /// </list>
    /// </returns>
    public static Result<CliOptions> Parse(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help"))
            return Result<CliOptions>.Fail(CliErrors.HelpRequested);

        var dict = args
            .Select(a => a.Split('=', 2))
            .Where(a => a.Length == 2)
            .ToDictionary(a => a[0], a => a[1]);

        if (!dict.TryGetValue("--employees", out var employees))
            return Result<CliOptions>.Fail(CliErrors.Missing("--employees"));

        if (!dict.TryGetValue("--branches", out var branches))
            return Result<CliOptions>.Fail(CliErrors.Missing("--branches"));

        if (!dict.TryGetValue("--clients", out var clients))
            return Result<CliOptions>.Fail(CliErrors.Missing("--clients"));

        var format = dict.GetValueOrDefault("--format", "csv");

        return Result<CliOptions>.Ok(new CliOptions(
            EmployeesPath: employees,
            BranchesPath: branches,
            ClientsPath: clients,
            Format: format.ToLowerInvariant()
        ));
    }
}
