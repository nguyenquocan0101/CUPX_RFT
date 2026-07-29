using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Services.Validations;

public class MatchEmail : ValidationAttribute
{
    private const string _pattern =
        @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

    public MatchEmail()
    {
        ErrorMessage = "Email is not in a valid format.";
    }

    public override bool IsValid(object value)
    {
        if (value is null) return true; // Allow [Required] to handle null

        string email = value as string;
        if (string.IsNullOrWhiteSpace(email)) return false;

        return Regex.IsMatch(email, _pattern);
    }
}