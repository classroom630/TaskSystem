namespace TaskSystem.API.Data.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string fullName);
        Task SendTaskAssignedEmailAsync(string email, string fullName, string taskTitle);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}