namespace Services.Dtos.Dashboard.Traffic.HourlyPeak;

public class HourlyPointDto
{
    public string Hour { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsPeak { get; set; }

    public int OrderCount { get; set; }
}