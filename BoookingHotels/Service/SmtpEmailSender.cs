using System.Net;
using System.Net.Mail;
using static BoookingHotels.Controllers.AuthController;

namespace BoookingHotels.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        public void Send(string to, string content)
        {
            var fromAddress = new MailAddress("nguyentuankiet2003py@gmail.com", "Booking Hotels");
            var toAddress = new MailAddress(to);
            const string fromPassword = "tlax ngrc fnxd jotl";
            const string subject = "Mã OTP xác thực tài khoản";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = content
            };
            smtp.Send(message);
        }
    }
}
