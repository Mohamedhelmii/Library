using System.Net;
using System.Net.Mail;

namespace Library.Verification
{
    public class VerificationConfirmation
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("mohamedhelmy0209@gmail.com", "ezchpymyximneosr"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage("mohamedhelmy0209@gmail.com", toEmail, subject, body);
            await smtpClient.SendMailAsync(mailMessage);
            //Console.WriteLine(smtpClient);
        }
    }
}
