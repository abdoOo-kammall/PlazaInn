using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlazaCore.ServiceContract;
using Shared.DTO.Dashboard;

namespace Plaza.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboard;

        public AdminDashboardController(IAdminDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<AdminDashboardSummaryDTO>> GetSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var summary = await _dashboard.GetDashboardSummaryAsync(fromDate, toDate, cancellationToken);
            return Ok(summary);
        }
    }
}
