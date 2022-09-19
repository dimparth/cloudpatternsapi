using cloudpatternsapi.interfaces;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace cloudpatternsapi.implementation
{
    public class EmailService : IEmailService
    {
        public EmailService()
        {

        }
        public void SendEmail(string subject, string? recipient, string? messageBody)
        {
            if (string.IsNullOrEmpty(subject)) throw new Exception("error");
            try
            {
                MailMessage message = new();
                SmtpClient smtp = new();
                message.From = new MailAddress("showbookingathens@gmail.com");
                message.To.Add(new MailAddress(recipient!));

                message.Subject = subject;

                message.Body = messageBody;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";

                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("showbookingathens@gmail.com", "hnci euxz xlem lfdy");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                smtp.EnableSsl = true;

                RemoteCertificateValidationCallback value = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }!;
                ServicePointManager.ServerCertificateValidationCallback = value;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
