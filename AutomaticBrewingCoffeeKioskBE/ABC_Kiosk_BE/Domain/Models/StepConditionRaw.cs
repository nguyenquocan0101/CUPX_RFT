using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class StepConditionRaw
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Expression { get; set; } = null!;
    }
}
