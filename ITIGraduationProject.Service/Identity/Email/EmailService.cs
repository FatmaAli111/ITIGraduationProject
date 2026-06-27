using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Service.Identity.Email
{
    public class EmailService:IEmailService
    {
        private readonly EmailSettings _emailSetting;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSetting, ILogger<EmailService> logger)
        {
            _emailSetting = emailSetting.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSetting.NameSender, _emailSetting.EmailSender));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSetting.SmtpServer, _emailSetting.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSetting.EmailSender, _emailSetting.AppPassword);
            var response = await client.SendAsync(message);
            _logger.LogInformation(
                "SMTP server accepted email to {Recipient}. Response: {Response}",
                to,
                response);
            await client.DisconnectAsync(true);
        }
    }
}
