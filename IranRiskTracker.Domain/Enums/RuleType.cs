namespace IranRiskTracker.Domain.Enums
{
    /// <summary>
    /// Rule application type. Similar to SignalType but kept separate for intent clarity.
    /// </summary>
    public enum RuleType
    {
        Include = 0,
        Exclude = 1,
        Dispute = 2,
        Experience = 3
    }
}
