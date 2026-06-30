using Application.Constants;
using Application.Services.Otp;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services.Otp
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private const int OtpExpiryMinutes = 5;

        public EmailService(IConfiguration configuration, IDistributedCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task SendOtpAsync(string email)
        {
            var code = Random.Shared.Next(1000, 9999).ToString();
            var cacheKey = GetCacheKey(email);

            await _cache.SetStringAsync(cacheKey, code, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpExpiryMinutes)
            });

            var subject = _configuration["EmailSettings:OtpSubject"] ?? "Reset Password";
            var body = EmailTemplates.OtpResetPassword(code, OtpExpiryMinutes);
            await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> ValidateOtpAsync(string email, string code)
        {
            var cacheKey = GetCacheKey(email);
            var cachedCode = await _cache.GetStringAsync(cacheKey);
            return cachedCode != null && cachedCode == code;
        }

        public async Task RemoveOtpAsync(string email)
        {
            var cacheKey = GetCacheKey(email);
            await _cache.RemoveAsync(cacheKey);
        }

        protected virtual async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"]!;
            var senderName = emailSettings["SenderName"]!;
            var smtpServer = emailSettings["SmtpServer"]!;
            var port = int.Parse(emailSettings["Port"]!);
            var password = emailSettings["Password"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string GetCacheKey(string email) => $"otp:reset:{email.ToLower()}";
    }
}
