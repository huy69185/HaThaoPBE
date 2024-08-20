using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _fromEmail = "huyh69185@gmail.com";
    private readonly string _appPassword = "wimnchrsoydixuds"; // Mật khẩu ứng dụng của bạn

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                client.EnableSsl = true;
                client.Timeout = 30000; // Thời gian chờ là 30 giây

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
        catch (SmtpException ex)
        {
            // Ghi log hoặc hiển thị chi tiết lỗi SMTP
            throw new InvalidOperationException("SMTP error occurred while sending email.", ex);
        }
        catch (Exception ex)
        {
            // Ghi log hoặc hiển thị chi tiết lỗi chung
            throw new InvalidOperationException("Error occurred while sending email.", ex);
        }
    }
}
