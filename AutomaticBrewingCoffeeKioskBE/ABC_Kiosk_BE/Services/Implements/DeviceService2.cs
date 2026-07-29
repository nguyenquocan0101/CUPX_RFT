
using Domain.CouchDbModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.CouchDbRepository;
using Services.Base;
using Services.Dtos.Device;
using Services.Interfaces;

namespace Services.Implements
{
    public class DeviceService2 : IDeviceService2
    {
        private readonly IDeviceDocumentRepository _deviceDocumentRepository;
        private readonly IDeviceStatusRepository _deviceStatusRepository;
        private readonly ILogger<DeviceService2> _logger;
        public DeviceService2(IDeviceDocumentRepository deviceDocumentRepository, IDeviceStatusRepository deviceStatusRepository, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeviceService2>();
            _deviceDocumentRepository = deviceDocumentRepository;
            _deviceStatusRepository = deviceStatusRepository;
        }
        public async Task<BaseResult<DeviceDocument[]>> GetAllDeviceDocsAsync(DeviceDocQueryDto query)
        {
            try
            {
                var deviceDocs = await _deviceDocumentRepository.GetAllAsync(query.DeviceModelId, query.WorkingStatus);
                var deviceDocIds = deviceDocs.ToDictionary(dd => dd.DeviceId, dd => dd);

                foreach (var deviceId in deviceDocIds.Keys)
                {
                    var deviceStatus = await _deviceStatusRepository.GetByIdAsync(deviceId);
                    deviceDocIds[deviceId].Status = deviceStatus?.Status ?? [];
                    deviceDocIds[deviceId].Labels = deviceStatus?.Labels ?? new Dictionary<string, string>();
                }

                return new BaseResult<DeviceDocument[]>
                {
                    IsSuccess = true,
                    Message = "Device documents retrieved successfully.",
                    ResponseRequest = [.. deviceDocIds.Values],
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device documents");
                return new BaseResult<DeviceDocument[]>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving device documents: {ex.Message}",
                    ResponseRequest = Array.Empty<DeviceDocument>(),
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
