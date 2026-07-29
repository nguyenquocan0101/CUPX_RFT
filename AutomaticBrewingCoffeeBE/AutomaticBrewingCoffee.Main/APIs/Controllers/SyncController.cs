using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Sync;
using Services.Dtos.SyncEvent;
using Services.Dtos.SyncTask;
using Services.Interfaces;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/syncs")]
    [ApiController]
    [TrimStrings]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("sync-kiosk")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult<BaseResult<string, SynchronizedKioskDataDto>>> SynchronizedKioskData(
            string kioskId)
        {
            var response = await _syncService.SynchronizedKioskData(kioskId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("override-kiosk")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult<BaseResult<string, OverridenKioskDataDto>>> OverridenKioskData(string kioskId)
        {
            var response = await _syncService.OverridenKioskData(kioskId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("events")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult<BaseResult<SyncEventQueryDto, Paginate<SyncEventDto>>>> GetSyncEvents(
            [FromQuery] SyncEventQueryDto syncEventQueryDto
        )
        {
            var response = await _syncService.GetSyncEvents(syncEventQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("tasks")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult<BaseResult<SyncTaskQueryDto, Paginate<SyncTaskDto>>>> GetSyncTasks(
            [FromQuery] SyncTaskQueryDto syncTaskQueryDto
        )
        {
            var response = await _syncService.GetSyncTasks(syncTaskQueryDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}