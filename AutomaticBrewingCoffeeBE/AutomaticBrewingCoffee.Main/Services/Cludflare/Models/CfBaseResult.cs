using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Cludflare.Models
{
    public class CfBaseResult<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public List<object> Errors { get; set; }
        public List<object> Messages { get; set; }

    }
}
