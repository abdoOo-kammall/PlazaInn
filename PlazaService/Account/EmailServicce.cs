using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PlazaCore.ServiceContract.Account;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody , List<(MemoryStream Stream, string FileName)>? attachments = null)
    {
        using var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
        {
            Port = int.Parse(_configuration["Email:Port"] ?? "587"),
            Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            ),
            EnableSsl = true,

        };
        htmlBody += _configuration["Email:Signature"];

        // Disposing the message also disposes every attachment stream added to it.
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:From"]!),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(to);
        if (attachments != null)
        {
            foreach (var att in attachments)
            {
                att.Stream.Position = 0;
                mailMessage.Attachments.Add(new Attachment(att.Stream, att.FileName));
            }
        }

        await smtpClient.SendMailAsync(mailMessage);
    }
}
