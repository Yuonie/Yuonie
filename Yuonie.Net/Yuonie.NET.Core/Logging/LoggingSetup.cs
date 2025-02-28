namespace Yuonie.NET.Core.Logging;

public static class LoggingSetup
{
    /// <summary>
    /// 系统日志注册
    /// </summary>
    /// <param name="services"></param>
    public static void AddLoggingSetup(this IServiceCollection services)
    {
        //日志监听
        services.AddMonitorLogging(options =>
        {
            options.IgnorePropertyNames = new[] { "Byte" };
            options.IgnorePropertyTypes = new[] { typeof(byte[]) };
        });
        // 控制台日志
        var consoleLog = App.GetConfig<bool>("Logging:Monitor:ConsoleLog", true);
        services.AddConsoleFormatter(options =>
        {
            options.DateFormat = "yyyy-MM-dd HH:mm:ss(zzz) dddd";
            options.WriteFilter = (logMsg) =>
            {
                return consoleLog;
            };
        });
        //写入日志文件
        var fileLog = App.GetConfig<bool>("Logging:File:Enabled", true);
        if (fileLog)
        {
            var loggingMonitorSettings = App.GetConfig<LoggingMonitorSettings>("Logging:Monitor", true);
            Array.ForEach(new[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error }, logLevel =>
            {
                services.AddFileLogging(options =>
                {
                    options.WithTraceId = true; // 显示线程Id
                    options.WithStackFrame = true; // 显示程序集
                    options.FileNameRule = fileName => string.Format(fileName, DateTime.Now, logLevel.ToString()); // 每天创建一个文件
                    options.WriteFilter = logMsg => logMsg.LogLevel == logLevel; // 日志级别
                    options.HandleWriteError = (writeError) => // 写入失败时启用备用文件
                    {
                        writeError.UseRollbackFileName(Path.GetFileNameWithoutExtension(writeError.CurrentFileName) + "-oops" + Path.GetExtension(writeError.CurrentFileName));
                    };
                    if (loggingMonitorSettings.JsonBehavior == JsonBehavior.OnlyJson)
                    {
                        options.MessageFormat = LoggerFormatter.Json;
                        options.MessageFormat = (logMsg) =>
                        {
                            var jsonString = logMsg.Context.Get("loggingMonitor");
                            return jsonString?.ToString();
                        };
                    }
                });
            });



        }
        // 日志写入数据库
        if (App.GetConfig<bool>("Logging:Database:Enabled", true))
        {
            services.AddDatabaseLogging<DatabaseLoggingWriter>(options =>
            {
                options.WithTraceId = true; // 显示线程Id
                options.WithStackFrame = true; // 显示程序集
                options.IgnoreReferenceLoop = false; // 忽略循环检测
                options.WriteFilter = (logMsg) =>
                {
                    return logMsg.LogName == "System.Logging.LoggingMonitor"; // 只写LoggingMonitor日志
                };
            });
        }


    }

}
