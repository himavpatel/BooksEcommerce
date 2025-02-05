using System.Net.Mail;
using System.Net;

namespace BooksEcommerce.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            bool status = false;
            try
            {

                var smtpSettings = _configuration.GetSection("EmailSettings");
                string smtpServer = smtpSettings["SmtpServer"];
                string senderEmailAddress = smtpSettings["SenderEmail"];
                string senderPassword = smtpSettings["SenderPassword"];
                int port = int.Parse(smtpSettings["Port"]);

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmailAddress),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // If the message contains HTML content
                };
                mailMessage.To.Add(new MailAddress(email));

                using var smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = port,
                    EnableSsl = true, // Ensure SSL is enabled for secure connections
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmailAddress, senderPassword)
                };

                await smtp.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Use a logging framework like Serilog or NLog in production
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }


    }
}
