using System;

namespace IranRiskTracker.Domain.ValueObjects
{
    /// <summary>
    /// Validated wrapper for a 0–100 percent risk value.
    /// </summary>
    public readonly struct RiskPercent
    {
        public decimal Value { get; }

        public RiskPercent(decimal value)
        {
            if (value < 0m || value > 100m) throw new ArgumentOutOfRangeException(nameof(value), "RiskPercent must be between 0 and 100");
            Value = value;
        }

        public override string ToString() => Value.ToString("0.##");
    }
}
