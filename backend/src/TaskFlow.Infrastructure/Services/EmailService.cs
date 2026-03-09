using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// SMTP を使用したメール送信サービス
/// </summary>
public class EmailService : EmailServiceInterface
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// EmailService を生成する
    /// </summary>
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// メール確認メールを送信する
    /// </summary>
    public async Task SendEmailVerificationAsync(string toEmail, string token)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        var verifyUrl = $"{frontendUrl}/verify-email?token={Uri.EscapeDataString(token)}";

        var subject = "【TaskFlow】メールアドレスの確認";
        var body = $"""
            TaskFlow にご登録いただきありがとうございます。

            以下のリンクをクリックしてメールアドレスを確認してください。

            {verifyUrl}

            このリンクは24時間有効です。
            心当たりのない場合は、このメールを無視してください。
            """;

        await sendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// パスワードリセットメールを送信する
    /// </summary>
    public async Task SendPasswordResetAsync(string toEmail, string token)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        var resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        var subject = "【TaskFlow】パスワードリセット";
        var body = $"""
            パスワードリセットのリクエストを受け付けました。

            以下のリンクをクリックしてパスワードを再設定してください。

            {resetUrl}

            このリンクは1時間有効です。
            心当たりのない場合は、このメールを無視してください。
            """;

        await sendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// メールを送信する
    /// </summary>
    private async Task sendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:SenderName"] ?? "TaskFlow",
            _configuration["Email:SenderAddress"] ?? "noreply@taskflow.local"));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        var host = _configuration["Email:SmtpHost"] ?? "localhost";
        var port = int.Parse(_configuration["Email:SmtpPort"] ?? "1025");

        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
