namespace Services.Local;

public sealed class LocalSeedOptions
{
    public bool Enabled { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string KioskApiKey { get; set; } = string.Empty;
    public string KioskBaseUrl { get; set; } = "http://localhost:5160";
}
