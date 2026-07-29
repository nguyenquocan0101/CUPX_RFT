namespace Services.Dtos.Step;

public class StepConditionRawDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Expression { get; set; } = null!;
}