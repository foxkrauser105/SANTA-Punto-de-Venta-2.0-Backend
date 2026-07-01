using Microsoft.Extensions.Configuration;
using SANTA.PoS.Business.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SANTA.PoS.Business.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendAsync(string subject, string htmlBody)
    {
        var host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
        var sender = _configuration["Email:SenderAddress"] ?? "";
        var password = _configuration["Email:SenderPassword"] ?? "";
        var ownerEmail = _configuration["Email:OwnerEmail"] ?? sender;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(sender, password)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(sender),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(ownerEmail);

        await client.SendMailAsync(mail);
    }
}
