using Application.DTOs;
using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                //create message
                var message = new MimeMessage();
                message.Sender = new MailboxAddress(_configuration["MailSettings:DisplayName"],
                   _configuration["MailSettings:EmailFrom"]);

                message.To.Add(MailboxAddress.Parse(emailRequestDto.To));
                message.Subject = emailRequestDto.Subject!;

                var builder = new BodyBuilder();

                if (emailRequestDto.IsHtmlBody)
                {
                    builder.HtmlBody = emailRequestDto.Body; //Using HTML Body
                }
                else
                {
                    builder.TextBody = emailRequestDto.Body; //using Plain text body
                }

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Connect(_configuration["MailSettings:SmtpHost"],
                    Convert.ToInt32(_configuration["MailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration["MailSettings:SmtpUser"],
                    _configuration["MailSettings:SmtpPass"]);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
