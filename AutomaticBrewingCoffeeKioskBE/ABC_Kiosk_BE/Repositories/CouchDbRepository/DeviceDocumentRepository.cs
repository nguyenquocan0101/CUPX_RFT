
using CouchDb.Domain.Enums;
using CouchDB.Driver.Extensions;
using Domain.CouchDbModels;
using Domain.Models;

namespace Repositories.CouchDbRepository
{
    public interface IDeviceDocumentRepository
    {
        Task<DeviceDocument?> GetByIdAsync(string deviceId);
        Task<DeviceDocument[]> GetArraysAsync(string? deviceModelId, EWorkingStatus? workingStatus);
        Task<List<DeviceDocument>> GetAllAsync(string? deviceModelId, EWorkingStatus? workingStatus);
        Task<List<DeviceDocument>> GetAllAsync();
        Task<bool> UpdateDeviceAsync(DeviceDocument document);
        Task<bool> AddFromDeviceAsync(Device device, Dictionary<string, object> status);
        Task<bool> DeleteAsync(string deviceId);
        Task<bool> UnlockDeviceDocAsync(string deviceId);
    }

    public class DeviceDocumentRepository : IDeviceDocumentRepository
    {
        private readonly KioskDbContext _context;
        public DeviceDocumentRepository(KioskDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceDocument?> GetByIdAsync(string deviceId)
        {
            return await _context.DeviceDocuments.FirstOrDefaultAsync(dd => dd.DeviceId == deviceId);
        }

        public async Task<DeviceDocument[]> GetArraysAsync(string? deviceModelId, EWorkingStatus? workingStatus)
        {
            var query = _context.DeviceDocuments.AsQueryable();
            if (!string.IsNullOrEmpty(deviceModelId)) {  
                query = query.Where(dd => dd.DeviceModelId == deviceModelId);
            }

            if (workingStatus != null)
            {
                query = query.Where(dd => dd.WorkingStatus == workingStatus);
            }
            return await query.ToArrayAsync();
        }
        public async Task<List<DeviceDocument>> GetAllAsync(string? deviceModelId, EWorkingStatus? workingStatus)
        {
            var query = _context.DeviceDocuments.AsQueryable();
            if (!string.IsNullOrEmpty(deviceModelId))
            {
                query = query.Where(dd => dd.DeviceModelId == deviceModelId);
            }

            if (workingStatus != null)
            {
                query = query.Where(dd => dd.WorkingStatus == workingStatus);
            }
            return await query.ToListAsync();
        }

        public async Task<List<DeviceDocument>> GetAllAsync()
        {
            return await _context.DeviceDocuments.ToListAsync();
        }

        public async Task<bool> UpdateDeviceAsync(DeviceDocument document)
        {
            var device = await GetByIdAsync(document.DeviceId);
            if (device == null) return false;

            device.DeviceModelId = document.DeviceModelId;
            device.SerialNumber = document.SerialNumber;
            device.Name = document.Name;
            device.Description = document.Description;
            device.WorkingStatus = document.WorkingStatus;
            device.Status = document.Status;
            await _context.DeviceDocuments.AddOrUpdateAsync(device);
            return true;
        }

        public async Task<bool> AddFromDeviceAsync(Device device, Dictionary<string, object> status)
        {
            var deviceDoc = new DeviceDocument
            {
                Id = device.DeviceId,
                DeviceModelId = device.DeviceModelId,
                SerialNumber = device.SerialNumber,
                Name = device.Name,
                Description = device.Description,
                WorkingStatus = EWorkingStatus.Idle,
                Status = status
            };
            await _context.DeviceDocuments.AddOrUpdateAsync(deviceDoc);
            return true;
        }

        public async Task<bool> DeleteAsync(string deviceId)
        {
            var device = await GetByIdAsync(deviceId);
            if (device == null) return false;

            await _context.DeviceDocuments.RemoveAsync(device);
            return true;
        }

        public async Task<bool> UnlockDeviceDocAsync(string deviceId)
        {
            var deviceToUnlock = await GetByIdAsync(deviceId);
            if (deviceToUnlock == null) return false;
            deviceToUnlock.WorkingStatus = EWorkingStatus.Idle;
            await _context.DeviceDocuments.AddOrUpdateAsync(deviceToUnlock);
            return true;
        }
    }
}
