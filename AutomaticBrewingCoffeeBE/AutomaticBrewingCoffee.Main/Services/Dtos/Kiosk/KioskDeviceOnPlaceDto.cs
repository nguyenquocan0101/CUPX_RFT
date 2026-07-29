namespace Services.Dtos.Kiosk;

public class KioskDeviceOnPlaceDto
{
    public string? DeviceId { get; set; }
    public string? DeviceModelId { get; set; }
    public string? SerialNumber { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? WorkingStatus { get; set; }
    public Dictionary<dynamic, dynamic>? Status { get; set; }
    public Dictionary<dynamic, dynamic>? Labels { get; set; }
    public string? Id { get; set; }
    public string? Rev { get; set; }
    public bool? Deleted { get; set; }
    public string? Conflicts { get; set; }
    public string? DeletedConflicts { get; set; }
    public int? LocalSequence { get; set; }
    public string? RevisionInfo { get; set; }
    public string? Revisions { get; set; }
    public List<object>? Attachments { get; set; }
}