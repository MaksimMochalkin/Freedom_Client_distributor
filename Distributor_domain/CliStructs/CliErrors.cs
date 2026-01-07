namespace Distributor_domain.CliStructs;

public static class CliErrors
{
    public static readonly Error HelpRequested =
        new("HELP", HelpText);

    public static Error Missing(string option) =>
        new("MISSING_ARGUMENT", $"Missing required argument: {option}\n\n{HelpText}");

    public static Error InvalidInputParam(string msg) =>
        new("INVALID_PARAM", $"Invalid input param: {msg}");

    public static string HelpText =>
"""
Usage:
  --employees=<path> --branches=<path> --clients=<path> [--format=<csv|excel|json>]

Options:
  --employees   Path to employees file (required)
  --branches    Path to branches file (required)
  --clients     Path to clients file (required)
  --format      Input file format (default: csv)
  --help        Show help
""";
}
