namespace IranRiskTracker.Domain.Enums
{
    /// <summary>
    /// Broad event categories used to classify events for scoring rules.
    /// </summary>
    public enum EventCategory
    {
        Unknown = 0,
        Protests = 1,
        Executions = 2,
        Nuclear = 3,
        Maritime = 4,
        Cyber = 5,
        Military = 6,
        Political = 7,
        Economic = 8
    }
}
