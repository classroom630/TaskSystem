using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using UserTaskManagement.Application.Interfaces;

namespace UserTaskManagement.Infrastructure.Services
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

        public async Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName)
        {
            var subject = "Welcome to Task Management System";
            var body = $@"
                <h2>Welcome to Task Management System!</h2>
                <p>Hello {firstName} {lastName},</p>
                <p>Welcome to our Task Management System. Your account has been successfully created.</p>
                <p>You can now log in and start managing your tasks efficiently.</p>
                <br>
                <p>Best regards,<br>Task Management Team</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendTaskAssignedEmailAsync(string toEmail, string firstName, string taskTitle)
        {
            var subject = "New Task Assigned";
            var body = $@"
                <h2>New Task Assignment</h2>
                <p>Hello {firstName},</p>
                <p>A new task has been assigned to you:</p>
                <p><strong>{taskTitle}</strong></p>
                <p>Please log in to the Task Management System to view the details and manage your task.</p>
                <br>
                <p>Best regards,<br>Task Management Team</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {firstName},</p>
                <p>You have requested to reset your password. Click the link below to reset your password:</p>
                <p><a href=""{resetLink}"">Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Task Management Team</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var smtpUsername = emailSettings["SmtpUsername"] ?? "";
                var smtpPassword = emailSettings["SmtpPassword"] ?? "";
                var fromEmail = emailSettings["FromEmail"] ?? "noreply@tasksystem.com";
                var fromName = emailSettings["FromName"] ?? "Task Management System";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Only connect and authenticate if SMTP settings are configured
                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    
                    _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                }
                else
                {
                    _logger.LogInformation("Email would be sent to {Email} with subject '{Subject}' (SMTP not configured)", toEmail, subject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                // Don't throw exception to prevent breaking the application flow
            }
        }
    }
}