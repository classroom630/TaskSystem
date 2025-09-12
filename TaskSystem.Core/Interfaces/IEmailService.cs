namespace TaskSystem.Core.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendEmailAsync(string to, string subject, string body);
}