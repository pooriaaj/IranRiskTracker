namespace IranRiskTracker.Domain.Enums
{
    /// <summary>
    /// Disambiguates whether EventId references a HistoricalEvent or LiveEvent row.
    /// Required for future EF Core FK configuration.
    /// </summary>
    public enum EventType
    {
        Historical = 0,
        Live = 1
    }
}
