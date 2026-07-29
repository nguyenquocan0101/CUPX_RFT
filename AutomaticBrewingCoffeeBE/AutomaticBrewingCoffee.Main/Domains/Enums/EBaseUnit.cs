using System.ComponentModel;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EBaseUnit
{
    [Description("Seconds")] Seconds,

    [Description("Milliliters")] Milliliters,

    [Description("Grams")] Grams,

    [Description("Piece")] Piece
}