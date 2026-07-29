namespace AutomaticBrewingCoffee.Domain.Models;

public class BaseModel
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; } = null!;
    public DateTime? DeletedDate { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;

    public void Delete()
    {
        UpdatedDate = DateTime.UtcNow;
        DeletedDate = DateTime.UtcNow;
        IsDeleted = true;
    }

    public void Restore()
    {
        UpdatedDate = DateTime.UtcNow;
        DeletedDate = null;
        IsDeleted = false;
    }
}