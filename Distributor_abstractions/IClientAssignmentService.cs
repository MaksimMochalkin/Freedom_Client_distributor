using Distributor_domain;
using Distributor_domain.Models;

namespace Distributor_abstractions;

public interface IClientAssignmentService
{
    /// <summary>
    /// Метод назначения клиентов на ближайший филиал и наименее загруженного менеджера
    /// </summary>
    /// <param name="clients"></param>
    /// <param name="employees"></param>
    /// <param name="branches"></param>
    /// <param name="token"></param>
    /// <returns>Возвращает список клиентов и менеджеров на которых были назначены клиенты с информацией насколько менеджер загружен</returns>
    Task<Result<IReadOnlyList<AssignmentResult>>> AssignAsync(
        IReadOnlyList<Client> clients,
        IReadOnlyList<Employee> employees,
        IReadOnlyList<Branch> branches,
        CancellationToken token = default);
}
