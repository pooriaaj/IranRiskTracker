using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    public interface IDashboardSummaryService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
