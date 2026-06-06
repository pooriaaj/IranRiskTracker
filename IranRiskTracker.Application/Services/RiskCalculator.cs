using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.Services
{
    /// <summary>
    /// Basic risk calculation service skeleton. Returns a fixed Unknown/0 score for now.
    /// Scoring logic will be implemented in later phases.
    /// </summary>
    public class RiskCalculator : IRiskCalculator
    {
        private readonly Application.Interfaces.ISeedDataProvider _seed;

        public RiskCalculator(Application.Interfaces.ISeedDataProvider seed)
        {
            _seed = seed;
        }

        public Task<RiskDto> GetCurrentRiskAsync()
        {
            // Simple baseline placeholder: compute a naive baseline score based on historical event counts
            var hist = _seed.GetHistoricalEvents().ToList();
            var indicators = _seed.GetIndicators().ToList();

            // baseline score = clamp( (total historical events / 100) * indicator coverage factor, 0..100 )
            var totalEvents = hist.Count;
            var indicatorFactor = indicators.Count > 0 ? indicators.Sum(i => i.Weight) : 1m;

            var raw = (decimal)totalEvents / 100m * indicatorFactor * 100m; // scale to percent
            var score = Math.Clamp((double)raw, 0.0, 100.0);

            var dto = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = score switch
                {
                    <= 10 => RiskLevel.Low,
                    <= 40 => RiskLevel.Medium,
                    <= 70 => RiskLevel.High,
                    _ => RiskLevel.Critical
                },
                Score = score,
                Summary = $"Phase 1 baseline computed from {totalEvents} historical events"
            };

            return Task.FromResult(dto);
        }
    }
}
