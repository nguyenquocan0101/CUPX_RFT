using MimeKit;
using Services.Email.Base;

namespace Services.Email;

public interface IEmailTransport
{
    Task SendAsync(
        MimeMessage message,
        SmtpConnectionOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class SmtpConnectionOptions
{
    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public bool RequiresAuthentication { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class MailKitEmailTransport : IEmailTransport
{
    public async Task SendAsync(
        MimeMessage message,
        SmtpConnectionOptions options,
        CancellationToken cancellationToken = default)
    {
        using var client = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            await client.ConnectAsync(options.Host, options.Port, options.UseSsl, cancellationToken);
            if (options.RequiresAuthentication)
            {
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(options.Username, options.Password, cancellationToken);
            }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, cancellationToken);
            throw;
        }
    }
}

public class EmailSender
{
    private readonly SmtpSettings _smtpSettings;
    private readonly IEmailTransport _transport;

    public EmailSender(SmtpSettings smtpSettings)
        : this(smtpSettings, new MailKitEmailTransport())
    {
    }

    public EmailSender(SmtpSettings smtpSettings, IEmailTransport transport)
    {
        _smtpSettings = smtpSettings ?? throw new ArgumentNullException(nameof(smtpSettings));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public Task SendEmailAsync(EmailMessage emailMessage) =>
        _transport.SendAsync(CreateEmail(emailMessage), new SmtpConnectionOptions
        {
            Host = _smtpSettings.Server,
            Port = _smtpSettings.Port,
            UseSsl = _smtpSettings.UseSsl,
            RequiresAuthentication = _smtpSettings.RequiresAuthentication,
            Username = _smtpSettings.Username,
            Password = _smtpSettings.Password
        });

    private MimeMessage CreateEmail(EmailMessage emailMessage)
    {
        var builder = new BodyBuilder { HtmlBody = emailMessage.Body };
        foreach (var attachment in emailMessage.Attachments)
            builder.Attachments.Add(attachment.Name, attachment.Value);

        var email = new MimeMessage
        {
            Subject = emailMessage.Subject,
            Body = builder.ToMessageBody()
        };
        email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        email.To.Add(new MailboxAddress(emailMessage.ToAddress.Split('@')[0], emailMessage.ToAddress));
        return email;
    }
}
