using BepInEx.Configuration;

namespace LalaDancer;

public static class Config {
    public class ConfigGroup(ConfigFile config, string group) {
        public ConfigEntry<T> Bind<T>(string key, T defaultValue, string description) {
            return config.Bind(group, key, defaultValue, description);
        }

        public ConfigEntry<T> Bind<T>(string key, T defaultValue, ConfigDescription description = null) {
            return config.Bind(group, key, defaultValue, description);
        }
    }

    public static class SfxPrediction {
        public static bool Enabled => enabled.Value;
        private static ConfigEntry<bool> enabled;
        public static void Initialize(ConfigGroup config) {
            enabled = config.Bind("Enabled", true, "Enable better sound effect prediction for enemy hits.");
        }
    }

    public static void Initialize(ConfigFile config) {
        SfxPrediction.Initialize(new(config, "SFX Prediction"));
    }
}
