namespace Distributor_domain.Dtos;

public sealed class ClientDto
{
    public string ClientId { get; set; } = default!;
    public string? Country { get; set; }
    public string? City { get; set; }
    public IReadOnlyList<string> Attributes { get; set; } = Array.Empty<string>();

    public ClientDto()
    {
    }
}
