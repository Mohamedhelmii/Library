// في مجلد Services/EmailService.cs

using Library.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // 1. قراءة مفتاح API من الإعدادات
        var apiKey = _config["SendGrid:ApiKey"];
        var senderEmail = _config["SendGrid:SenderEmail"];

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("SendGrid API Key is not configured.");
        }

        var client = new SendGridClient(apiKey);

        var msg = new SendGridMessage()
        {
            From = new EmailAddress(senderEmail, "Library Management System"),
            Subject = subject,
            PlainTextContent = body,
            HtmlContent = body // يمكنك إرسال محتوى HTML أيضاً
        };

        msg.AddTo(new EmailAddress(toEmail));

        // 2. إرسال الإيميل عبر API
        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            // التعامل مع أخطاء الإرسال
            Console.WriteLine($"Error sending email: {response.StatusCode}");
            // يمكن رمي استثناء أو تسجيل الخطأ
        }
    }
}