using System.Linq;
using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;

namespace IranRiskTracker.Application.Services
{
    public class DashboardSummaryService : IDashboardSummaryService
    {
        private readonly IRiskCalculator _calculator;

        public DashboardSummaryService(IRiskCalculator calculator)
        {
            _calculator = calculator;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var risk = await _calculator.GetCurrentRiskAsync();

            var percent = (int)System.Math.Round(risk.Score);
            percent = System.Math.Clamp(percent, 1, 100);

            var top = risk.Contributions.OrderByDescending(c => c.WeightedContribution).Take(5)
                .Select(c => new DashboardTopContributorDto
                {
                    IndicatorKey = c.IndicatorKey,
                    IndicatorName = c.IndicatorName,
                    Category = c.Category,
                    WeightedContribution = c.WeightedContribution,
                    BaseScore = c.BaseScore,
                    SeverityAdjustedBaseScore = c.SeverityAdjustedBaseScore,
                    Explanation = c.Explanation
                }).ToList();

            return new DashboardSummaryDto
            {
                Score = risk.Score,
                ScorePercent = percent,
                Level = risk.Level,
                PreviousScore = risk.PreviousScore,
                ScoreChange = risk.ScoreChange,
                ScoreTrend = risk.ScoreTrend,
                Timestamp = risk.Timestamp,
                Summary = risk.Summary,
                TopContributors = top
            };
        }
    }
}
