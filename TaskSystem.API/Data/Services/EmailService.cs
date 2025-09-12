using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TaskSystem.API.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Welcome to TaskSystem!";
            var htmlMessage = $@"
                <h2>Welcome to TaskSystem, {fullName}!</h2>
                <p>Your account has been successfully created.</p>
                <p>You can now log in and start managing your tasks.</p>
                <p>Best regards,<br/>TaskSystem Team</p>";
                
            await SendEmailAsync(email, subject, htmlMessage);
        }
        
        public async Task SendTaskAssignedEmailAsync(string email, string fullName, string taskTitle)
        {
            var subject = "New Task Assigned";
            var htmlMessage = $@"
                <h2>Hello {fullName},</h2>
                <p>A new task has been assigned to you:</p>
                <p><strong>{taskTitle}</strong></p>
                <p>Please log in to view the details and start working on it.</p>
                <p>Best regards,<br/>TaskSystem Team</p>";
                
            await SendEmailAsync(email, subject, htmlMessage);
        }
        
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["FromName"], 
                    emailSettings["FromEmail"]));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };
                message.Body = bodyBuilder.ToMessageBody();
                
                using var client = new SmtpClient();
                
                // For development, we'll log instead of sending actual emails
                if (_configuration.GetValue<bool>("EmailSettings:UseMockService", true))
                {
                    _logger.LogInformation("Mock Email Sent to: {Email}, Subject: {Subject}", email, subject);
                    await Task.CompletedTask;
                    return;
                }
                
                await client.ConnectAsync(
                    emailSettings["SmtpHost"], 
                    emailSettings.GetValue<int>("SmtpPort"),
                    SecureSocketOptions.StartTls);
                    
                await client.AuthenticateAsync(
                    emailSettings["Username"], 
                    emailSettings["Password"]);
                    
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                // Don't throw exception to avoid breaking the main flow
            }
        }
    }
}