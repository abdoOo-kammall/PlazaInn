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

    public async Task SendEmailAsync(string to, string subject, string htmlBody , List<IFormFile>? attachments = null)
    {
        var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
        {
            Port = int.Parse(_configuration["Email:Port"]),
            Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            ),
            EnableSsl = true,

        };
        htmlBody += _configuration["Email:Signature"];


        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:From"]),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(to);
        if (attachments != null)
        {
            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                     var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;

                    var attachment = new Attachment(ms, file.FileName);
                    mailMessage.Attachments.Add(attachment);
                }
            }
        }

        await smtpClient.SendMailAsync(mailMessage);
        foreach (var attachment in mailMessage.Attachments)
        {
            attachment.ContentStream.Dispose();
        }
    }
}
