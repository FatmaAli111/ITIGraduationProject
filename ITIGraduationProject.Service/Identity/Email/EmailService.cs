using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using MailKit.Net.Smtp;
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

        public EmailService(IOptions<EmailSettings> emailSetting)
        {
            _emailSetting = emailSetting.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSetting.EmailSender, _emailSetting.NameSender));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart()
            {
                Text = body
            };
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSetting.SmtpServer, _emailSetting.Port, false);
            await client.AuthenticateAsync(_emailSetting.EmailSender, _emailSetting.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
