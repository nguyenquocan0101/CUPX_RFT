namespace Services.CapRabbitMQ.Messages.Email;

public class EmailInvitationCapMessage
{
    public string OrganizationName { get; set; } = null!;
    public string AccountEmail { get; set; } = null!;
    public string AccountPassword { get; set; } = null!;
}