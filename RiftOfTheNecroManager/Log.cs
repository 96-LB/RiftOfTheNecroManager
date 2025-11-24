using BepInEx.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace RiftOfTheNecroManager;


public static class Log {
    private static Dictionary<Assembly, ManualLogSource> Logs { get; } = [];

    private static Assembly GetCallingAssembly() {
        var stackTrace = new StackTrace();
        var thisAssembly = Assembly.GetExecutingAssembly();
        for(int i = 1; i < stackTrace.FrameCount; i++) {
            var frame = stackTrace.GetFrame(i);
            var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
            if(assembly != null && assembly != thisAssembly) {
                return assembly;
            }
        }
        return thisAssembly;
    }

    internal static ManualLogSource GetLog() {
        var assembly = GetCallingAssembly();
        if(!Logs.TryGetValue(assembly, out var logSource)) {
            logSource = Logger.CreateLogSource(assembly.GetName().Name ?? "Unknown");
            Logs[assembly] = logSource;
        }
        return logSource;
    }

    internal static ManualLogSource SetLog(Assembly assembly, ManualLogSource logSource) => Logs[assembly] = logSource;

    public static void AtLevel(LogLevel level, object message) => GetLog().Log(level, message.ToString());
    public static void Debug(object message) => AtLevel(LogLevel.Debug, message);
    public static void Info(object message) => AtLevel(LogLevel.Info, message);
    public static void Message(object message) => AtLevel(LogLevel.Message, message);
    public static void Warning(object message) => AtLevel(LogLevel.Warning, message);
    public static void Error(object message) => AtLevel(LogLevel.Error, message);
    public static void Fatal(object message) => AtLevel(LogLevel.Fatal, message);
}
