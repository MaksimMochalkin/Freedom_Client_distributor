namespace Distributor_domain.Models;

public sealed class Branch
{
    public string Name { get; }
    public string City { get; }
    public double Latitude { get; }
    public double Longitude { get; }

    public Branch(string name, string city, double latitude, double longitude)
    {
        Name = name;
        City = city;
        Latitude = latitude;
        Longitude = longitude;
    }
}
