using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RiftOfTheNecroManager;


abstract public class Setting {
    protected static Dictionary<Assembly, List<Setting>> SettingsByAssembly { get; } = [];
    public Assembly Assembly { get; }

    public Setting(Assembly assembly) {
        Assembly = assembly;

        if(!SettingsByAssembly.TryGetValue(Assembly, out var settings)) {
            SettingsByAssembly[Assembly] = settings = [];
        }

        settings.Add(this);
    }
    
    internal static void RegisterAssembly(ConfigFile config, Assembly assembly) {
        foreach(var type in assembly.GetTypes()) {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        if(SettingsByAssembly.TryGetValue(assembly, out var settings)) {
            foreach(var setting in settings) {
                setting.Bind(config);
            }
        }
    }

    public abstract ConfigEntryBase Bind(ConfigFile config);
}


public class Setting<T>(string group, string key, T defaultValue, string description, AcceptableValueBase? acceptableValues = null, object[]? tags = null) : Setting(Util.GetCallingAssembly(typeof(Setting<T>))) {
    public string Group { get; } = group;
    public string Key { get; } = key;
    public T DefaultValue { get; } = defaultValue;
    public ConfigDescription Description { get; } = new(description, acceptableValues, tags);


    private ConfigEntry<T>? entry;
    public ConfigEntry<T> Entry => entry ?? throw new InvalidOperationException("Setting is not bound.");

    public static implicit operator T(Setting<T> setting) => setting.Entry.Value;
    public static implicit operator ConfigEntry<T>(Setting<T> setting) => setting.Entry;

    public override ConfigEntryBase Bind(ConfigFile config) {
        return entry = config.Bind(Group, Key, DefaultValue, Description);
    }
}
