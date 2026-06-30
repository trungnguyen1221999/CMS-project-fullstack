namespace Application.Constants
{
    public static class EmailTemplates
    {
        public static string OtpResetPassword(string code, int expiryMinutes = 5)
        {
            return """
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8" />
                    <style>
                        body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #ffffff; padding: 40px 20px; }
                        .container { max-width: 480px; margin: 0 auto; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 40px 32px; }
                        .header { text-align: center; margin-bottom: 28px; }
                        .header h2 { color: #1a202c; margin: 0; font-size: 24px; font-weight: 600; }
                        .text { color: #4a5568; font-size: 15px; line-height: 1.6; }
                        .code-box { background: #ffffff; border: 2px solid #3b82f6; border-radius: 10px; padding: 24px; text-align: center; margin: 28px 0; }
                        .code { font-size: 36px; font-weight: 700; letter-spacing: 10px; color: #3b82f6; }
                        .expiry { color: #64748b; font-size: 14px; text-align: center; margin-top: 8px; }
                        .warning { color: #ef4444; font-size: 13px; margin-top: 20px; padding: 12px; background: #fef2f2; border-radius: 6px; text-align: center; }
                        .footer { color: #94a3b8; font-size: 12px; text-align: center; margin-top: 28px; border-top: 1px solid #e2e8f0; padding-top: 20px; }
                    </style>
                </head>
                <body>
                    <div class="container">
                        <div class="header">
                            <h2>🔐 Reset Your Password</h2>
                        </div>
                        <p class="text">You have requested to reset your password. Use the verification code below to proceed:</p>
                        <div class="code-box">
                            <span class="code">{{CODE}}</span>
                        </div>
                        <p class="expiry">This code will expire in <strong>{{EXPIRY}} minutes</strong>.</p>
                        <p class="warning">⚠️ If you did not request this, please ignore this email. Do not share this code with anyone.</p>
                        <div class="footer">
                            <p>&copy; CMS Project - Kai Nguyen</p>
                        </div>
                    </div>
                </body>
                </html>
                """
                .Replace("{{CODE}}", code)
                .Replace("{{EXPIRY}}", expiryMinutes.ToString());
        }
    }
}
