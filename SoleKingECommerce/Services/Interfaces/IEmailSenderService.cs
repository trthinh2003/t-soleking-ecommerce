namespace SoleKingECommerce.Services.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendSmsAsync(string number, string message);
    }
}
