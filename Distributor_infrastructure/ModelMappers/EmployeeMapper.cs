using Distributor_domain.Dtos;
using Distributor_domain.Models;

namespace Distributor_infrastructure.ModelMappers;

/// <summary>
/// Маппер dto модели сотрудника на бизнес модель
/// </summary>
public static class EmployeeMapper
{
    public static Employee ToDomain(this EmployeeDto dto)
        => new(
            id: dto.EmployeeId.ToString(),
            branch: dto.Branch,
            initialClients: dto.ClientCount,
            skills: dto.Skills
        );
}
