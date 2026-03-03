public sealed class Employee
{
    public string Id { get; }
    public string Branch { get; }
    public IReadOnlyCollection<string> Skills { get; }

    // Единственный источник истины
    private int _workload;

    // 0 = free, 1 = reserved
    private int _reserved;

    public Employee(
        string id,
        string branch,
        int initialClients,
        IReadOnlyCollection<string> skills)
    {
        Id = id;
        Branch = branch;
        Skills = skills;
        _workload = initialClients;
    }

    public bool HasSkill(string skill) => Skills.Contains(skill);

    public int TotalWorkload => Volatile.Read(ref _workload);

    /// <summary>
    /// Атомарно увеличивает нагрузку
    /// </summary>
    public int AssignClient()
        => Interlocked.Increment(ref _workload);

    public bool TryReserve()
        => Interlocked.CompareExchange(ref _reserved, 1, 0) == 0;

    public void ReleaseReservation()
        => Interlocked.Exchange(ref _reserved, 0);
}