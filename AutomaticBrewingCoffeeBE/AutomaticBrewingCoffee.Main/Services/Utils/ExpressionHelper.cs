using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Services.Utils;
using Services.Dtos.Step;

namespace Services.Utils;

public static class ExpressionHelper
{
    public static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        if (first == null) return second;
        if (second == null) return first;

        var parameter = Expression.Parameter(typeof(T));

        var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Chuyển StepExpressionDto thành biểu thức string (ví dụ: context.side == "left")
    /// </summary>
    public static string ToExpressionString(StepExpressionDto? dto)
    {
        if (dto == null) return string.Empty;

        var left = FormatPart(dto.Left);
        var right = FormatPart(dto.Right);
        var op = MapOperatorToSymbol(dto.Operator);
        return $"{left} {op} {right}";
    }

    private static string FormatPart(ExpressionPartDto part)
    {
        if (part.Type == "Literal")
        {
            return part.Value switch
            {
                string s => $"\"{s}\"",
                bool b => b.ToString().ToLower(),
                _ => part.Value?.ToString() ?? ""
            };
        }

        // variable
        return part.Value?.ToString() ?? "";
    }

    public static StepExpressionDto? ParseExpressionString(string? expression)
    {
        if (expression is null)
        {
            return null;
        }

        var supportedOperators = new[] { "==", "!=", ">=", "<=", ">", "<" };

        foreach (var op in supportedOperators)
        {
            var index = expression.IndexOf(op, StringComparison.Ordinal);
            if (index > 0)
            {
                var leftRaw = expression[..index].Trim();
                var rightRaw = expression[(index + op.Length)..].Trim();

                return new StepExpressionDto
                {
                    Left = new ExpressionPartDto
                    {
                        Type = "Variable",
                        Value = leftRaw
                    },
                    Operator = MapSymbolToOperator(op), // "==" → "Equal"
                    Right = new ExpressionPartDto
                    {
                        Type = "Literal",
                        Value = ParseLiteralValue(rightRaw)
                    }
                };
            }
        }

        return new StepExpressionDto
        {
            Left = new ExpressionPartDto
            {
                Type = "variable",
                Value = "leftRaw"
            },
            Operator = "MapSymbolToOperator(op)", // "==" → "Equal"
            Right = new ExpressionPartDto
            {
                Type = "literal",
                Value = "ParseLiteralValue(rightRaw)"
            }
        };
    }

    private static string MapSymbolToOperator(string symbol) => symbol switch
    {
        "==" => nameof(EOperation.Equal),
        "!=" => nameof(EOperation.NotEqual),
        ">" => nameof(EOperation.GreaterThan),
        ">=" => nameof(EOperation.GreaterThanOrEqual),
        "<" => nameof(EOperation.LessThan),
        "<=" => nameof(EOperation.LessThanOrEqual),
        _ => throw new ArgumentException($"Unknown operator symbol: {symbol}")
    };

    private static string MapOperatorToSymbol(string symbol) => symbol switch
    {
        nameof(EOperation.Equal) => "==",
        nameof(EOperation.NotEqual) => "!=",
        nameof(EOperation.GreaterThan) => ">",
        nameof(EOperation.GreaterThanOrEqual) => ">=",
        nameof(EOperation.LessThan) => "<",
        nameof(EOperation.LessThanOrEqual) => "<=",
        _ => throw new ArgumentException($"Unknown operator symbol: {symbol}")
    };

    private static object ParseLiteralValue(string value)
    {
        // Nếu là string dạng "abc"
        if (value.StartsWith("\"") && value.EndsWith("\""))
            return value.Trim('\"');

        // Nếu là int
        if (int.TryParse(value, out var i)) return i;

        // Nếu là bool
        if (bool.TryParse(value, out var b)) return b;

        // fallback
        return value;
    }
}