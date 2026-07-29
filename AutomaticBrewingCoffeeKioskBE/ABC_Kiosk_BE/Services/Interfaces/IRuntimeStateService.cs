
namespace Services.Interfaces
{
    public interface IRuntimeStateService
    {
        Task SetMaintenanceAsync(bool on);
        Task<bool> IsMaintenanceAsync();
    }
}
