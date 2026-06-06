using System;

namespace IranRiskTracker.Domain.ValueObjects
{
    /// <summary>
    /// Validated wrapper for source credibility score between 0.0 and 1.0.
    /// </summary>
    public readonly struct CredibilityScore
    {
        public decimal Value { get; }

        public CredibilityScore(decimal value)
        {
            if (value < 0m || value > 1m) throw new ArgumentOutOfRangeException(nameof(value), "CredibilityScore must be between 0.0 and 1.0");
            Value = value;
        }

        public override string ToString() => Value.ToString("0.##");
    }
}
