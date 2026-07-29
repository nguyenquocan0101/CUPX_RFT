namespace Services.Dtos.Menu;

public class MenuInsideDto
{
    public string MenuId { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;
}