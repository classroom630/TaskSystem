namespace UserTaskManagement.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName);
        Task SendTaskAssignedEmailAsync(string toEmail, string firstName, string taskTitle);
        Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink);
    }
}