using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.CustomExceptions
{
    public class NotFoundException(string? msg = "Not Found Exception") : Exception(msg)
    {
    }
}
