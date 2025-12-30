namespace Distributor_domain.CliStructs;

public sealed record CliOptions(
string EmployeesPath,
string BranchesPath,
string ClientsPath,
string Format
);
