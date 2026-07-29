using MimeKit;
using Services.Email;
using Services.Email.Base;

namespace Services.Tests.Local.EmailWebhook;

public sealed class EmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_PreservesHtmlRecipientSubjectAndAttachments()
    {
        var transport = new RecordingEmailTransport();
        var settings = new SmtpSettings
        {
            Host = "127.0.0.1",
            Port = 1025,
            SenderName = "CupX Local",
            SenderEmail = "no-reply@cupx.local",
            UseSsl = false,
            RequiresAuthentication = false
        };
        var sender = new EmailSender(settings, transport);
        var attachment = EmailAttachment.Create(new byte[] { 1, 2, 3 }, "receipt.txt");

        await sender.SendEmailAsync(EmailMessage.Create(
            "admin@cupx.local",
            "<strong>Local message</strong>",
            "Local subject",
            attachment));

        Assert.NotNull(transport.Message);
        Assert.Equal("Local subject", transport.Message!.Subject);
        Assert.Equal("admin@cupx.local", transport.Message.To.Mailboxes.Single().Address);
        var multipart = Assert.IsType<Multipart>(transport.Message.Body);
        Assert.Contains("Local message", multipart.OfType<TextPart>().First(x => x.IsHtml).Text);
        Assert.Contains("receipt.txt", transport.Message.Attachments.Select(x => x.ContentDisposition?.FileName));
        Assert.Equal("127.0.0.1", transport.Options!.Host);
        Assert.Equal(1025, transport.Options.Port);
        Assert.False(transport.Options.UseSsl);
        Assert.False(transport.Options.RequiresAuthentication);
    }

    [Fact]
    public void SmtpSettings_MapsLegacyAndMailpitPropertyNames()
    {
        var settings = new SmtpSettings { Server = "smtp.example.test", Username = "legacy-user" };

        Assert.Equal("smtp.example.test", settings.Host);
        Assert.Equal("legacy-user", settings.UserName);
    }

    private sealed class RecordingEmailTransport : IEmailTransport
    {
        public MimeMessage? Message { get; private set; }
        public SmtpConnectionOptions? Options { get; private set; }

        public Task SendAsync(
            MimeMessage message,
            SmtpConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            Message = message;
            Options = options;
            return Task.CompletedTask;
        }
    }
}
