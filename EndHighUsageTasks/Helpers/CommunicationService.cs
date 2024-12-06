using EndHighUsageTasks.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace EndHighUsageTasks.Helper;

/// <summary>
/// Provides communication services, including email functionality.
/// </summary>
/// <param name="logger">Logger for capturing application logs.</param>
/// <param name="configuration">Configuration object for accessing app settings.</param>
/// <exception cref="ArgumentNullException">Thrown if logger or configuration is null.</exception>
public class CommunicationService(ILogger<CommunicationService> logger, IConfiguration configuration)
{
    private readonly ILogger<CommunicationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Sends an email using Gmail's SMTP server.
    /// </summary>
    /// <param name="emailTo">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body of the email.</param>
    /// <returns>True if the email is sent successfully; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if email settings are not found in the configuration.</exception>
    public bool SendEmail(string body, string? emailTo = null, string? subject = null)
    {
        // Retrieve email settings from configuration
        EmailSettings? _emailSettings =
            _configuration.GetSection("EmailSettings").Get<EmailSettings>()
            ?? throw new InvalidOperationException("Failed to load EmailSettings from configuration.");

        try
        {
            // Create the email message
            var message = new MimeMessage
            {
                From = { new MailboxAddress("Sender", _emailSettings.EmailFrom) },
                To = { new MailboxAddress(emailTo, emailTo ?? _emailSettings.EmailTo) },
                Subject = subject ?? _emailSettings.EmailSubject,
                Body = new TextPart("html") // Set the body as HTML
                {
                    Text = body
                }
            };

            using var client = new SmtpClient();

            // Connect to the SMTP server
            client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

            // Authenticate with the SMTP server
            client.Authenticate(_emailSettings.EmailFrom, _emailSettings.GmailApplicationPassword);

            // Send the email
            client.Send(message);
            client.Disconnect(true);

            // Log success
            _logger.LogInformation("Email sent successfully to {emailTo}.", emailTo);
            return true;
        }
        catch (Exception ex)
        {
            // Log error details
            _logger.LogError(ex, "Error sending email to {emailTo}.", emailTo);
            return false;
        }
    }
}
