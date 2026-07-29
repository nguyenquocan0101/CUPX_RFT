using Services.Base;
using Services.Dtos.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IKioskSyncService
    {
        Task<BaseResult> SyncKioskData(SyncActionDto data);
        Task<BaseResult> SyncOverridenKioskData(OverridenKioskDataSyncDto data);
    }
}
