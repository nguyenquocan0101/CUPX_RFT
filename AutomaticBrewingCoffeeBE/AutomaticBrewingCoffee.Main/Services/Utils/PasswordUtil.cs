namespace Services.Utils;

public static class PasswordUtil
{
    public static string GenerateTemporaryPassword(int length = 10)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Không dùng I, O
        const string lower = "abcdefghijkmnopqrstuvwxyz"; // Không dùng l
        const string digits = "23456789"; // Tránh 0 và 1 dễ nhầm
        const string specials = "!@#$%^&*";

        var allChars = upper + lower + digits + specials;
        var rand = new Random();

        var password = new char[length];
        password[0] = upper[rand.Next(upper.Length)];
        password[1] = lower[rand.Next(lower.Length)];
        password[2] = digits[rand.Next(digits.Length)];
        password[3] = specials[rand.Next(specials.Length)];

        for (int i = 4; i < length; i++)
            password[i] = allChars[rand.Next(allChars.Length)];

        // Trộn ngẫu nhiên mảng ký tự
        return new string(password.OrderBy(x => rand.Next()).ToArray());
    }
}