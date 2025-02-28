using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Yuonie.NET.Core.Option;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Furion.VirtualFileServer;
using Furion.Schedule;
using IGeekFan.AspNetCore.Knife4jUI;
using Furion.SpecificationDocument;
using Furion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yuonie.NET.Core.Logging;

namespace Yuonie.NET.Core;

public class Startup : AppStartup
{
    /// <summary>
    /// 依赖注入配置
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        #region 自定义配置选项
        services.AddProjectOptions();
        #endregion
        #region 全局缓存注册
        // 缓存注册
        services.AddCacheSetup();
        #endregion
        #region ORM注册
        // SqlSugar
        services.AddSqlSugarSetup();
        #endregion
        #region 允许跨域
        // 允许跨域
        services.AddCorsAccessor();
        #endregion
        #region 任务队列以及持久化
        //// 任务队列 
        //services.AddTaskQueue();
        //// 任务调度
        //services.AddSchedule(options =>
        //{
        //    options.AddPersistence<DbJobPersistence>(); // 添加作业持久化器
        //});
        #endregion
        #region 图像处理
        // 图像处理
        services.AddImageSharp();
        #endregion
        #region 即时通讯
        services.AddSignalR();
        #endregion
        #region 验证码
        // 验证码
        services.AddCaptcha();
        #endregion
        #region 配置Nginx转发获取客户端真实IP
        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        #endregion
        #region 配置Json
        static void SetNewtonsoftJsonSetting(JsonSerializerSettings setting)
        {
            setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            setting.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            setting.DateFormatString = "yyyy-MM-dd HH:mm:ss"; // 时间格式化
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // 忽略循环引用
            // setting.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 解决动态对象属性名大写
            // setting.NullValueHandling = NullValueHandling.Ignore; // 忽略空值
            // setting.Converters.AddLongTypeConverters(); // long转string（防止js精度溢出） 超过17位开启
            // setting.MetadataPropertyHandling = MetadataPropertyHandling.Ignore; // 解决DateTimeOffset异常
            // setting.DateParseHandling = DateParseHandling.None; // 解决DateTimeOffset异常
            // setting.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }); // 解决DateTimeOffset异常
        };
        services.AddControllersWithViews()
                    .AddAppLocalization()
                    .AddNewtonsoftJson(options => SetNewtonsoftJsonSetting(options.SerializerSettings))
                    .AddInjectWithUnifyResult<AdminResultProvider>();
        #endregion
        #region 系统日志
        // 控制台logo
        services.AddConsoleLogo();
        services.AddLoggingSetup();
        #endregion
    }

    /// <summary>
    /// 中间件管道配置
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseForwardedHeaders();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseForwardedHeaders();
            app.UseHsts();
        }
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Yuonie.NET", "Yuonie.NET");
            await next();
        });

        // 图像处理
        app.UseImageSharp();

        // 特定文件类型（文件后缀）处理
        var contentTypeProvider = FS.GetFileExtensionContentTypeProvider();
        // contentTypeProvider.Mappings[".文件后缀"] = "MIME 类型";
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider
        });
        // 添加状态码拦截中间件
        app.UseUnifyResultStatusCodes();

        // 启用多语言，必须在 UseRouting 之前
        app.UseAppLocalization();

        // 路由注册
        app.UseRouting();

        // 启用跨域，必须在 UseRouting 和 UseAuthentication 之间注册
        app.UseCorsAccessor();

        // 启用鉴权授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 配置Swagger-Knife4UI（路由前缀一致代表独立，不同则代表共存）
        app.UseKnife4UI(options =>
        {
            options.RoutePrefix = "kapi";
            foreach (var groupInfo in SpecificationDocumentBuilder.GetOpenApiGroups())
            {
                options.SwaggerEndpoint("/" + groupInfo.RouteTemplate, groupInfo.Title);
            }
        });

        app.UseInject(string.Empty);

        app.UseEndpoints(endpoints =>
        {
            // 注册集线器
            endpoints.MapHubs();

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

    }
}
