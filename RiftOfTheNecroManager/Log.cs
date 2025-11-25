using BepInEx.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace RiftOfTheNecroManager;


public static class Log {
    private static Dictionary<Assembly, ManualLogSource> Logs { get; } = [];

    internal static ManualLogSource GetLog() {
        var assembly = Util.GetCallingAssembly(typeof(Log));
        if(!Logs.TryGetValue(assembly, out var logSource)) {
            logSource = BepInEx.Logging.Logger.CreateLogSource(assembly.GetName().Name ?? "Unknown");
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
    
    // utility methods

    private static void PrintAllComponents(GameObject gameObject, int depth = 1) {
        foreach(var component in gameObject.GetComponents<Component>()) {
            Log.Info($"{new string(' ', depth * 2)}• {component.GetType().Name}");
        }
    }

    internal static void PrintAllComponents(GameObject gameObject) {
        Log.Info($"Components of [{gameObject}]:");
        PrintAllComponents(gameObject, 1);
    }
    internal static void PrintAllComponents(Transform transform) {
        PrintAllComponents(transform.gameObject);
    }

    internal static void PrintAllComponents(Component component) {
        PrintAllComponents(component.gameObject);
    }

    private static void PrintAllChildren(Transform transform, int depth, bool recursive = false, bool components = false) {
        if(components) {
            PrintAllComponents(transform.gameObject, depth + 1);
        }
        foreach(Transform child in transform) {
            Log.Info($"{new string(' ', depth * 2)}○ {child.name}");
            if(recursive) {
                PrintAllChildren(child, depth + 1, recursive, components);
            }
        }
    }

    internal static void PrintAllChildren(Transform transform, bool recursive = false, bool components = false) {
        Log.Info($"Children of [{transform}]:");
        PrintAllChildren(transform, 1, recursive, components);
    }

    internal static void PrintAllChildren(GameObject gameObject, bool recursive = false, bool components = false) {
        PrintAllChildren(gameObject.transform, recursive, components);
    }

    internal static void PrintAllChildren(Component component, bool recursive = false, bool components = false) {
        PrintAllChildren(component.gameObject, recursive, components);
    }
}
