using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos.Sync;
using Services.Interfaces;
using static MassTransit.ValidationResultExtensions;

namespace Kiosk.ApiService.Controllers
{
    [Route("/api/v1")]
    [ApiController]
    public class KioskSyncController : ControllerBase
    {
        private readonly IKioskSyncService _syncService;
        private readonly ILogger<KioskSyncController> _logger;

        public KioskSyncController(IKioskSyncService syncService, ILogger<KioskSyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        [HttpPost("synchronized-data")]
        public async Task<IActionResult> SyncKioskData([FromBody] SyncActionDto dto)
        {
            _logger.LogInformation("Received sync request for kiosk data.");
          var result = await _syncService.SyncKioskData(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("overridden-data")]
        public async Task<IActionResult> SyncOverridenKioskData([FromBody] OverridenKioskDataSyncDto dto)
        {
            _logger.LogInformation("Received overriden sync request for kiosk data.");
            var result = await _syncService.SyncOverridenKioskData(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
