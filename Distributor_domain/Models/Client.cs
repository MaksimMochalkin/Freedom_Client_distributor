namespace Distributor_domain.Models;

public sealed class Client
{
    public string Id { get; }
    public string? Country { get; }
    public string? City { get; }
    public IReadOnlyCollection<string> Attributes { get; }

    public bool IsVip => Attributes.Contains("VIP");

    public Client(string id, string? country, string? city, IReadOnlyCollection<string> attributes)
    {
        Id = id;
        Country = country;
        City = city;
        Attributes = attributes;
    }
}
