using Microsoft.Extensions.DependencyInjection;

namespace Yuonie.NET.Core.Option;

public static class ProjectOptionsSetup
{
    /// <summary>
    /// 注册项目配置选项
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddProjectOptions(this IServiceCollection services)
    {
        services.AddConfigurableOptions<DbConnectionOptions>();
        services.AddConfigurableOptions<SnowIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<SnowIdOptions>();
        services.AddConfigurableOptions<CryptogramOptions>();
        services.AddConfigurableOptions<EmailOptions>();
        

        return services;
    }
}
