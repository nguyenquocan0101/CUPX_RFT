using System.ComponentModel;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EOperation
{
    [Description("==")] Equal,

    [Description("!=")] NotEqual,

    [Description(">")] GreaterThan,

    [Description(">=")] GreaterThanOrEqual,

    [Description("<")] LessThan,

    [Description("<=")] LessThanOrEqual,
}