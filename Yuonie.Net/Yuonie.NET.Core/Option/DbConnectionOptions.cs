namespace Yuonie.NET.Core;

/// <summary>
/// 表示数据库连接的配置设置。
/// </summary>
public class DbConnectionOptions : IConfigurableOptions<DbConnectionOptions>
{
    /// <summary>
    /// 是否启用控制台输出SQL语句。
    /// </summary>
    public bool EnableConsoleSql { get; set; }

    /// <summary>
    /// 主数据库的连接ID。
    /// </summary>
    public string MainDB { get; set; }

    /// <summary>
    /// 数据库连接配置的列表。
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; }

    public void PostConfigure(DbConnectionOptions options, IConfiguration configuration)
    {
        foreach (var dbConfig in options.ConnectionConfigs)
        {
            if (dbConfig.ConfigId == null || string.IsNullOrWhiteSpace(dbConfig.ConfigId.ToString()))
                dbConfig.ConfigId = SqlSugarConst.MainConfigId;
        }
    }
}

/// <summary>
/// 表示单个数据库连接的配置。
/// </summary>
public class DbConnectionConfig:ConnectionConfig
{
    /// <summary>
    /// 配置的唯一标识符。
    /// </summary>
    public string ConnId { get; set; }

    /// <summary>
    /// 与数据库初始化相关的设置。
    /// </summary>
    public DbSettings DbSettings { get; set; }

    /// <summary>
    /// 与表初始化相关的设置。
    /// </summary>
    public TableSettings TableSettings { get; set; }

    /// <summary>
    /// 与种子数据初始化相关的设置。
    /// </summary>
    public SeedSettings SeedSettings { get; set; }
}

/// <summary>
/// 数据库相关设置。
/// </summary>
public class DbSettings
{
    /// <summary>
    /// 是否启用数据库初始化。
    /// </summary>
    public bool EnableInitDb { get; set; }

    /// <summary>
    /// 是否启用数据库表结构差异日志记录。
    /// </summary>
    public bool EnableDiffLog { get; set; }

    /// <summary>
    /// 是否启用驼峰命名转换为下划线命名。
    /// </summary>
    public bool EnableUnderLine { get; set; }
}

/// <summary>
/// 表初始化相关设置。
/// </summary>
public class TableSettings
{
    /// <summary>
    /// 是否启用表初始化。
    /// </summary>
    public bool EnableInitTable { get; set; }

    /// <summary>
    /// 是否启用表的增量更新。
    /// </summary>
    public bool EnableIncreTable { get; set; }
}

/// <summary>
/// 种子数据初始化相关设置。
/// </summary>
public class SeedSettings
{
    /// <summary>
    /// 是否启用种子数据初始化。
    /// </summary>
    public bool EnableInitSeed { get; set; }

    /// <summary>
    /// 是否启用种子数据的增量更新。
    /// </summary>
    public bool EnableIncreSeed { get; set; }
}
