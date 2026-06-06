using System.Threading.Tasks;
using IranRiskTracker.Application.DTOs;

namespace IranRiskTracker.Application.Interfaces
{
    /// <summary>
    /// Service abstraction for computing risk snapshots.
    /// Phase 1/2: skeleton only, scoring not implemented.
    /// </summary>
    public interface IRiskCalculator
    {
        /// <summary>
        /// Produces the current risk snapshot.
        /// </summary>
        Task<RiskDto> GetCurrentRiskAsync();
    }
}
