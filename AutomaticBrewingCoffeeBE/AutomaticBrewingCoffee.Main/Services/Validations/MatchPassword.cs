using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Services.Validations;

public class MatchPassword : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        var password = value as string;
        if (string.IsNullOrEmpty(password)) return false;
        
        var hasMinimum8Chars = password.Length >= 8;
        var hasUpperChar = Regex.IsMatch(password, "[A-Z]");
        var hasLowerChar = Regex.IsMatch(password, "[a-z]");
        var hasNumber = Regex.IsMatch(password, @"\d");
        var hasSpecialChar = Regex.IsMatch(password, @"[\W_]");

        if (hasMinimum8Chars && hasUpperChar && hasLowerChar && hasNumber && hasSpecialChar)
        {
            return true;
        }

        ErrorMessage =
            "Password must be at least 8 characters and contain upper, lower, number, and special character.";
        return false;
    }
}