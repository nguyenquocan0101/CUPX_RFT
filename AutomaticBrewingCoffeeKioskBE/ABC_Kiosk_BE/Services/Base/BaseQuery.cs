namespace Services.Base;

public class BaseQuery
{
    public string? FilterBy { get; set; }
    public string? FilterQuery { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool IsAsc { get; set; } = true;
}