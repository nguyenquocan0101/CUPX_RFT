using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Services.CapRabbitMQ.Messages.Email;
using Services.CapRabbitMQ.Topics;
using Services.Email;
using Services.Email.Base;
using Services.Email.Templates;

namespace Services.CapRabbitMQ.Subscribers;

public class EmailCapSubscriber : ICapSubscribe
{
    private readonly EmailSender _emailSender;
    private readonly EmailTemplateHandler _emailTemplateHandler;
    private readonly ILogger<EmailCapSubscriber> _logger;

    public EmailCapSubscriber(EmailSender emailSender, EmailTemplateHandler emailTemplateHandler,
        ILoggerFactory loggerFactory)
    {
        _emailSender = emailSender;
        _emailTemplateHandler = emailTemplateHandler;
        _logger = loggerFactory.CreateLogger<EmailCapSubscriber>();
    }

    [CapSubscribe(EmailCapTopic.EmailInvitation)]
    public async Task HandleEmailInvitation(EmailInvitationCapMessage message)
    {
        try
        {
            _logger.LogInformation("HandleEmailInvitation: Handle email invitation template");
            var invitationTemplate =
                await _emailTemplateHandler.GetTemplateAsync(EmailTemplateConstants.InvitationEmail);
            var emailBody = _emailTemplateHandler.ReplaceInTemplate(
                invitationTemplate,
                new Dictionary<string, string>()
                {
                    {
                        "{OrganizationRepresent}", "quản trị viên"
                    },
                    {
                        "{OrganizationName}", message.OrganizationName
                    },
                    {
                        "{Email}", message.AccountEmail
                    },
                    {
                        "{Password}", message.AccountPassword
                    },
                    {
                        "{WebsiteUrl}", "http://localhost:3000/login"
                    },
                }
            );

            _logger.LogInformation("HandleEmailInvitation: Send email invitation");
            await _emailSender.SendEmailAsync(EmailMessage.Create(
                toAddress: message.AccountEmail,
                body: emailBody,
                subject: "Invitation To CupX"
            ));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleEmailInvitation");
            throw;
        }
    }

    [CapSubscribe(EmailCapTopic.EmailBan)]
    public async Task HandleEmailBan(dynamic data)
    {
        return;
    }
}