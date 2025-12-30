using Distributor_domain.Dtos;
using Distributor_domain.Models;

namespace Distributor_infrastructure.ModelMappers;

/// <summary>
/// Маппер dto модели клиента на бизнес модель
/// </summary>
public static class ClientMapper
{
    public static Client ToDomain(this ClientDto dto)
        => new(
            id: dto.ClientId,
            country: dto.Country,
            city: dto.City,
            attributes: dto.Attributes
        );
}
