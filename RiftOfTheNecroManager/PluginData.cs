using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RiftOfTheNecroManager.BeatmapEvents;
using UnityEngine;

namespace RiftOfTheNecroManager;


public static class PluginData {
    private static Dictionary<Assembly, RiftPluginInternal> PluginsByAssembly { get; } = [];
    
    // TODO: docstrings?
    public static RiftPluginInternal Instance => GetPlugin();
    public static PluginInfo Info => Instance.Info;
    public static RiftPluginInfo Metadata => RiftPluginInfo.Of(Instance.Info);
    public static string Name => Metadata.Name;
    public static string BasePath => Path.GetDirectoryName(Application.dataPath);
    public static string DataPath => Path.Combine(BasePath, Name);
    public static Type Type => Instance.GetType();
    public static Assembly Assembly => Type.Assembly;
    
    public static RiftPluginInternal GetPlugin(Assembly? assembly = null) {
        assembly ??= Util.GetCallingAssembly(typeof(RiftPluginInternal));
        if(PluginsByAssembly.TryGetValue(assembly, out var plugin)) {
            return plugin;
        }
        throw new KeyNotFoundException($"No plugin found for assembly {assembly.FullName}.");
    }
    
    internal static void RegisterAssembly(Assembly assembly, RiftPluginInternal plugin) {
        if(PluginsByAssembly.TryGetValue(assembly, out var existingPlugin)) {
            var message = $"Multiple plugins, or multiple copies of the same plugin, have been detected from the same plugin assembly as {existingPlugin.Metadata.InfoString}. Each assembly may only instantiate one plugin.";
            Log.Fatal(message);
            throw new InvalidOperationException(message);
        }
        PluginsByAssembly[assembly] = plugin;
    }
}
