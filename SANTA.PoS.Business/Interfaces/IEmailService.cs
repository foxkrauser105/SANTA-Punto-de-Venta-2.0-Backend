namespace SANTA.PoS.Business.Interfaces;

public interface IEmailService
{
    Task SendAsync(string subject, string htmlBody);
}
