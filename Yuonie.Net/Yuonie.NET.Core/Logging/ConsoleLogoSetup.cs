namespace Yuonie.NET.Core;

/// <summary>
/// 控制台logo
/// </summary>
public static class ConsoleLogoSetup
{
    public static void AddConsoleLogo(this IServiceCollection services)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(@"
 \ \/ /_ _____  ___  (_)__ 
  \  / // / _ \/ _ \/ / -_)
  /_/\_,_/\___/_//_/_/\__/ 
");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"源码地址: https://github.com/Yuonie/Yuonie");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"让.NET更简单、更通用、更流行！");
    }
}