namespace Distributor_domain.Models;

public sealed class Employee
{
    public string Id { get; }
    public string Branch { get; }
    public IReadOnlyCollection<string> Skills { get; }

    private int _currentClients;
    private int _newClients;

    public Employee(string id, string branch, int clientCount, IReadOnlyCollection<string> skills)
    {
        Id = id;
        Branch = branch;
        Skills = skills;
        _currentClients = clientCount;
    }

    public bool HasSkill(string skill) => Skills.Contains(skill);

    public int TotalWorkload => Volatile.Read(ref _currentClients) + Volatile.Read(ref _newClients);

    public int AssignClient()
    {
        Interlocked.Increment(ref _newClients);
        return TotalWorkload;
    }
}
