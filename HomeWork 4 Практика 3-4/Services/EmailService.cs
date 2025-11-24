using System.Net;
using System.Net.Mail;

namespace MiniHTTPServer2.Services
{
    static class EmailService
    {
        public static void SendEmail(string to, string subject, string message)
        {
            AppContext.SetSwitch("System.Net.DisableIPv6", true);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var smtp = new SmtpClient("smtp.yandex.ru", 25);
            smtp.Credentials = new NetworkCredential("Gamzat0v.Rustam@yandex.ru", "Пароль");
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            var from = new MailAddress("Gamzat0v.Rustam@yandex.ru", "Рустам");
            
            var tu = new MailAddress(to);
            var mailMess = new MailMessage(from, tu);
            mailMess.Attachments.Add(new Attachment("C:/Users/gamza/Documents/Attachments.popa"));
            mailMess.Subject = subject;
            mailMess.Body = message;
            smtp.Send(mailMess);
        }
    }
}
