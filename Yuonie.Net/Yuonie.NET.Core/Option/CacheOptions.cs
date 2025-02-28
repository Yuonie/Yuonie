namespace Yuonie.NET.Core;


public class CacheOptions : IConfigurableOptions<CacheOptions>
{
    /// <summary>
    /// 全局使用的前缀
    /// </summary>
    public string Prefix { get; set; }
    /// <summary>
    /// 缓存的类型
    /// </summary>
    public string CacheType { get; set; }
    /// <summary>
    /// Redis配置
    /// </summary>
    public Redis Redis { get; set; }

    public void PostConfigure(CacheOptions options, IConfiguration configuration)
    {
        options.Prefix = string.IsNullOrWhiteSpace(options.Prefix) ? "" : options.Prefix.Trim();
    }
}

/// <summary>
/// Redis配置
/// </summary>
public class Redis
{
    /// <summary>
    /// 是否使用Redis作定时任务
    /// </summary>
    public bool UseTimedTask { get; set; }

    /// <summary>
    /// 队列、以及缓存默认的DbIndex
    /// </summary>
    public int DefaultDatabase { get; set; }
    /// <summary>
    /// Redis默认数据库
    /// </summary>
    public int TimedTaskIndex { get; set; } = 0;
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// 最大消息大小
    /// </summary>
    public int MaxMessageSize { get; set; }
}
