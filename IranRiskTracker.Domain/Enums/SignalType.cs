namespace IranRiskTracker.Domain.Enums
{
    /// <summary>
    /// Signal handling types produced by scoring rules.
    /// </summary>
    public enum SignalType
    {
        Included = 0,
        Excluded = 1,
        Disputed = 2,
        ExperienceBased = 3
    }
}
