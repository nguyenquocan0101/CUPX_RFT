using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.Validations;

namespace Services.Dtos.KioskDevice;

public class ChangeKioskDeviceStatusDto
{
    public KioskDeviceStatus Status { get; set; }
}