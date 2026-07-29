namespace Services.Email.Base;

public class SmtpSettings
{
    public string Server { get; set; } = "127.0.0.1";

    public int Port { get; set; }

    public string SenderName { get; set; } = "CUPX";

    public string SenderEmail { get; set; } = "no-reply@cupx.local";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Host { get => Server; set => Server = value; }
    public string UserName { get => Username; set => Username = value; }
    public bool UseSsl { get; set; } = true;
    public bool RequiresAuthentication { get; set; } = true;
}
