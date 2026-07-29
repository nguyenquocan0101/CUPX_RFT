
using CouchDb.Domain.Enums;
using CouchDB.Driver.Extensions;
using Domain.CouchDbModels;

namespace Repositories.CouchDbRepository
{
    public interface IDeviceStatusRepository
    {
        Task<DeviceStatusDocument?> GetByIdAsync(string deviceId);
        Task<DeviceStatusDocument?> GetByDocIdAsync(string id);
        //Task<DeviceStatusDocument[]> GetArraysAsync(string? deviceModelId);
        Task<bool> UpdateDeviceStatusAsync(DeviceStatusDocument document);
    }

    public class DeviceStatusRepository : IDeviceStatusRepository
    {
        private readonly KioskDbContext _context;
        public DeviceStatusRepository(KioskDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceStatusDocument?> GetByIdAsync(string deviceId)
        {
            return await _context.DeviceStatuses.FirstOrDefaultAsync(ds => ds.DeviceId == deviceId);
        }

        public async Task<DeviceStatusDocument?> GetByDocIdAsync(string id)
        {
            return await _context.DeviceStatuses.FirstOrDefaultAsync(ds => ds.Id == id);
        }

        //public async Task<DeviceStatusDocument[]> GetArraysAsync(string? deviceId)
        //{
        //    var query = _context.DeviceStatuses.AsQueryable();
        //    if (!string.IsNullOrEmpty(deviceId))
        //    {
        //        query = query.Where(ds => ds.Id == deviceId);
        //    }
        //    return await query.ToArrayAsync();
        //}
        public async Task<bool> UpdateDeviceStatusAsync(DeviceStatusDocument document)
        {
            Console.WriteLine($"Update status of device {document.DeviceId}");
            await _context.DeviceStatuses.AddOrUpdateAsync(document);
            return true;
        }


    }
}
