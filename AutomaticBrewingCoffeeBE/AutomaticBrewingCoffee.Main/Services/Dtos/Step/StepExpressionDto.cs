using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Step;

public class StepExpressionDto
{
    public ExpressionPartDto Left { get; set; } = default!;

    [MatchEnum(typeof(EOperation))]
    public string Operator { get; set; } = default!; // "==", "!=", ">", ">=", "<", "<=", "&&", "||"

    public ExpressionPartDto Right { get; set; } = default!;
}

public class ExpressionPartDto
{
    [MatchEnum(typeof(EExpressionType))] public string Type { get; set; } = null!;
    public object Value { get; set; } = null!;
}