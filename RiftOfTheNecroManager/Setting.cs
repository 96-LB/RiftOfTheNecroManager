using BepInEx.Configuration;
using System;

namespace RiftOfTheNecroManager;


public class Setting<T>(string key, T defaultValue, string description, AcceptableValueBase? acceptableValues = null, object[]? tags = null) {
    private ConfigEntry<T>? entry;
    public ConfigEntry<T> Entry => entry ?? throw new InvalidOperationException("Setting is not bound.");

    public static implicit operator T(Setting<T> setting) => setting.Entry.Value;
    public static implicit operator ConfigEntry<T>(Setting<T> setting) => setting.Entry;

    public ConfigEntry<T> Bind(ConfigFile config, string group) {
        return entry = config.Bind(group, key, defaultValue, new ConfigDescription(description, acceptableValues, tags));
    }
}
