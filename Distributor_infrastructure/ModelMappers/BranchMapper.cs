using Distributor_domain.Dtos;
using Distributor_domain.Models;

namespace Distributor_infrastructure.ModelMappers;

/// <summary>
/// Маппер dto модели филиала на бизнес модель
/// </summary>
public static class BranchMapper
{
    public static Branch ToDomain(this BranchDto dto)
        => new(
            name: dto.Branch,
            city: dto.City,
            latitude: dto.Latitude,
            longitude: dto.Longitude
        );
}
