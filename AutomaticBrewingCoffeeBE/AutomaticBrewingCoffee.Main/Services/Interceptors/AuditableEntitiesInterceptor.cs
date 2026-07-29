using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Services.Interceptors.Tasking;

namespace Services.Interceptors
{
    public class AuditableEntitiesInterceptor(ILogger<AuditableEntitiesInterceptor> logger) : SaveChangesInterceptor
    {
        private readonly AuditSyncEventTasking _auditSyncEventTasking = new AuditSyncEventTasking();

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (eventData.Context is null)
            {
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            await _auditSyncEventTasking.AuditSyncEventAsync(eventData);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}