using System.Net;
using System.Net.Mail;

namespace AAUP_LabMaster.EntityManager
{
    public class NotificationManager
    {
        public void SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("My Server")
            {
                Port = 587,
                Credentials = new NetworkCredential("MyEmail ", "password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("MyEmail"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
        }
    }
}
