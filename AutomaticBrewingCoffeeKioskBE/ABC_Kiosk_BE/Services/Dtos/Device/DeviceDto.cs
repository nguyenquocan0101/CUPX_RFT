using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Services.Dtos.Device;

public class DeviceDto
{
    public string? DeviceId { get; set; }

    public string? SerialNumber { get; set; } 
    public string? Name { get; set; }

    public string? Description { get; set; }

    public DeviceStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
    public decimal RX { get; set; }
    public decimal RY { get; set; }
    public decimal RZ { get; set; }
    public decimal J1 { get; set; }
    public decimal J2 { get; set; }
    public decimal J3 { get; set; }
    public decimal J4 { get; set; }
    public decimal J5 { get; set; }
    public decimal J6 { get; set; }
}