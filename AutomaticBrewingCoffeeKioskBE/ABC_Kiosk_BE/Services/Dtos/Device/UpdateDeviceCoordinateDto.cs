using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Device
{
    public class UpdateDeviceCoordinateDto
    {

        [Range(-999.999, 999.999, ErrorMessage = "X coordinate must be between -999.999 and 999.999")]
        public decimal? X { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "Y coordinate must be between -999.999 and 999.999")]
        public decimal? Y { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "Z coordinate must be between -999.999 and 999.999")]
        public decimal? Z { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "RX coordinate must be between -999.999 and 999.999")]
        public decimal? RX { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "RY coordinate must be between -999.999 and 999.999")]
        public decimal? RY { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "RZ coordinate must be between -999.999 and 999.999")]
        public decimal? RZ { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J1 coordinate must be between -999.999 and 999.999")]
        public decimal? J1 { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J2 coordinate must be between -999.999 and 999.999")]
        public decimal? J2 { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J3 coordinate must be between -999.999 and 999.999")]
        public decimal? J3 { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J4 coordinate must be between -999.999 and 999.999")]
        public decimal? J4 { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J5 coordinate must be between -999.999 and 999.999")]
        public decimal? J5 { get; set; }
        [Range(-999.999, 999.999, ErrorMessage = "J6 coordinate must be between -999.999 and 999.999")]
        public decimal? J6 { get; set; }
    }
}
