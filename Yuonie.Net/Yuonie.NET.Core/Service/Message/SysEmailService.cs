using MailKit.Net.Smtp;
using MimeKit;

namespace Yuonie.NET.Core.Service;


/// <summary>
/// 系统邮件发送服务
/// </summary>
[ApiDescriptionSettings(Order = 370)]
public class SysEmailService : IDynamicApiController, ITransient
{
    private readonly EmailOptions _emailOptions;

    public SysEmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="content"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    [DisplayName("发送邮件")]
    public async Task SendEmail([Required] string content, string title = "Admin.NET 系统邮件")
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOptions.DefaultFromEmail, _emailOptions.DefaultFromEmail));
        message.To.Add(new MailboxAddress(_emailOptions.DefaultToEmail, _emailOptions.DefaultToEmail));
        message.Subject = title;
        message.Body = new TextPart("html")
        {
            Text = content
        };

        using var client = new SmtpClient();
        client.Connect(_emailOptions.Host, _emailOptions.Port, _emailOptions.EnableSsl);
        client.Authenticate(_emailOptions.UserName, _emailOptions.Password);
        client.Send(message);
        client.Disconnect(true);

        await Task.CompletedTask;
    }
}