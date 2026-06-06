using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Computes current risk snapshots for API consumers.
    /// </summary>
    public interface IRiskCalculator
    {
        /// <summary>
        /// Produces the current risk snapshot.
        /// </summary>
        Task<RiskDto> GetCurrentRiskAsync();
    }
}
