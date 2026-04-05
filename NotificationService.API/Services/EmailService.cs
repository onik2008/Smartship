using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["SenderName"] ?? "SmartShip Logistics",
            emailSettings["SenderEmail"]!));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        if (isHtml)
            bodyBuilder.HtmlBody = body;
        else
            bodyBuilder.TextBody = body;

        message.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            emailSettings["SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(emailSettings["SmtpPort"] ?? "587"),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            emailSettings["SenderEmail"]!,
            emailSettings["SenderPassword"]!);

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", toEmail, subject);
    }
}
