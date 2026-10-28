using Shared.DTO.Dashboard;

namespace PlazaCore.ServiceContract
{
    public interface IAdminDashboardService
    {
        /// <summary>
        /// Read-only admin analytics. Booking metrics use ServiceDate when it is set, otherwise CreatedAt.
        /// </summary>
        Task<AdminDashboardSummaryDTO> GetDashboardSummaryAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);
    }
}
