namespace Distributor_domain.Dtos;

public class BranchDto
{
    public string Branch { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; } = default!;

    public BranchDto()
    {
    }
}
