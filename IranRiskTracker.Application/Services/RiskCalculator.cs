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
        public Task<RiskDto> GetCurrentRiskAsync()
        {
            // Placeholder implementation that returns a default value.
            var dto = new RiskDto
            {
                Timestamp = DateTime.UtcNow,
                Level = RiskLevel.Unknown,
                Score = 0.0,
                Summary = "Phase 1: scoring not implemented"
            };

            return Task.FromResult(dto);
        }
    }
}
