using Distributor_domain.CliStructs;
using Distributor_domain.Models;

namespace Distributor_domain;

public sealed record AssignmentContext
{
    public IReadOnlyList<Client> Clients { get; init; } = [];
    public IReadOnlyList<Employee> Employees { get; init; } = [];
    public IReadOnlyList<Branch> Branches { get; init; } = [];

    public IReadOnlyList<AssignmentResult>? Assignments { get; init; }

    public CliOptions? CliOptions { get; init; }
}

