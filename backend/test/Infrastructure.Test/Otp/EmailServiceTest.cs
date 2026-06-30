using Infrastructure.Services.Otp;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Test.Otp
{
    public class EmailServiceTest
    {
        private readonly IDistributedCache _cache;
        private readonly TestableEmailService _emailService;

        public EmailServiceTest()
        {
            _cache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions())
            );

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EmailSettings:OtpSubject"] = "Reset Password - CMS",
                    ["EmailSettings:SenderEmail"] = "test@cms.com",
                    ["EmailSettings:SenderName"] = "CMS",
                    ["EmailSettings:SmtpServer"] = "localhost",
                    ["EmailSettings:Port"] = "587",
                    ["EmailSettings:Password"] = "fake"
                })
                .Build();

            _emailService = new TestableEmailService(config, _cache);
        }

        // ===== SendOtpAsync TESTS =====

        [Fact]
        public async Task SendOtpAsync_StoresCodeInCache()
        {
            // Act
            await _emailService.SendOtpAsync("user@example.com");

            // Assert
            var cached = await _cache.GetStringAsync("otp:reset:user@example.com");
            Assert.NotNull(cached);
            Assert.Matches(@"^\d{4}$", cached);
        }

        [Fact]
        public async Task SendOtpAsync_CallsSendEmail()
        {
            // Act
            await _emailService.SendOtpAsync("user@example.com");

            // Assert
            Assert.Single(_emailService.SentEmails);
            Assert.Equal("user@example.com", _emailService.SentEmails[0].To);
            Assert.Equal("Reset Password - CMS", _emailService.SentEmails[0].Subject);
            Assert.False(string.IsNullOrEmpty(_emailService.SentEmails[0].Body));
        }

        [Fact]
        public async Task SendOtpAsync_EmailBodyContainsOtpCode()
        {
            // Act
            await _emailService.SendOtpAsync("user@example.com");

            // Assert
            var cached = await _cache.GetStringAsync("otp:reset:user@example.com");
            Assert.Contains(cached!, _emailService.SentEmails[0].Body);
        }

        [Fact]
        public async Task SendOtpAsync_UsesConfigSubject_WhenConfigured()
        {
            // Act
            await _emailService.SendOtpAsync("user@example.com");

            // Assert
            Assert.Equal("Reset Password - CMS", _emailService.SentEmails[0].Subject);
        }

        [Fact]
        public async Task SendOtpAsync_NormalizesEmailToLower()
        {
            // Act
            await _emailService.SendOtpAsync("User@Example.COM");

            // Assert
            var cached = await _cache.GetStringAsync("otp:reset:user@example.com");
            Assert.NotNull(cached);
        }

        // ===== ValidateOtpAsync TESTS =====

        [Fact]
        public async Task ValidateOtpAsync_CorrectCode_ReturnsTrue()
        {
            // Arrange
            await _cache.SetStringAsync("otp:reset:user@example.com", "1234");

            // Act
            var result = await _emailService.ValidateOtpAsync("user@example.com", "1234");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateOtpAsync_WrongCode_ReturnsFalse()
        {
            // Arrange
            await _cache.SetStringAsync("otp:reset:user@example.com", "1234");

            // Act
            var result = await _emailService.ValidateOtpAsync("user@example.com", "5678");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateOtpAsync_NoCodeInCache_ReturnsFalse()
        {
            // Act
            var result = await _emailService.ValidateOtpAsync("user@example.com", "1234");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateOtpAsync_CaseInsensitiveEmail()
        {
            // Arrange
            await _cache.SetStringAsync("otp:reset:user@example.com", "9999");

            // Act
            var result = await _emailService.ValidateOtpAsync("User@Example.COM", "9999");

            // Assert
            Assert.True(result);
        }

        // ===== RemoveOtpAsync TESTS =====

        [Fact]
        public async Task RemoveOtpAsync_RemovesCodeFromCache()
        {
            // Arrange
            await _cache.SetStringAsync("otp:reset:user@example.com", "1234");

            // Act
            await _emailService.RemoveOtpAsync("user@example.com");

            // Assert
            var cached = await _cache.GetStringAsync("otp:reset:user@example.com");
            Assert.Null(cached);
        }

        [Fact]
        public async Task RemoveOtpAsync_NonExistentKey_DoesNotThrow()
        {
            // Act & Assert - should not throw
            await _emailService.RemoveOtpAsync("nobody@example.com");
        }
    }

    /// <summary>
    /// Testable subclass that overrides SMTP sending to capture emails in memory.
    /// </summary>
    internal class TestableEmailService : EmailService
    {
        public List<(string To, string Subject, string Body)> SentEmails { get; } = [];

        public TestableEmailService(IConfiguration configuration, IDistributedCache cache)
            : base(configuration, cache) { }

        protected override Task SendEmailAsync(string to, string subject, string body)
        {
            SentEmails.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }
}
