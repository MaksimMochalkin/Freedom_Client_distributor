namespace Distributor_domain.Dtos;

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string Branch { get; set; } = default!;
    public int ClientCount { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = Array.Empty<string>();

    public EmployeeDto()
    {
    }
}

