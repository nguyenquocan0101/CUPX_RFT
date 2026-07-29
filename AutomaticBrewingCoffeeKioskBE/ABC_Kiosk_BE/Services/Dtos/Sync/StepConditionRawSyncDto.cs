using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Sync
{
    public class StepConditionRawSyncDto
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Expression { get; set; } = null!;
    }
}
