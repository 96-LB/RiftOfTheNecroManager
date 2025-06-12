using BepInEx.Configuration;
using UnityEngine;

namespace RiftOfTheNecroManager;

public enum SampleEnum {
    Foo,
    Bar,
    Baz
}

public static class Config {
    public class ConfigGroup(ConfigFile config, string group) {
        public ConfigEntry<T> Bind<T>(string key, T defaultValue, string description, AcceptableValueBase acceptableValues = null, params object[] tags) {
            return config.Bind(group, key, defaultValue, new ConfigDescription(description, acceptableValues, tags));
        }
    }

    public static class Samples {
        public static ConfigEntry<bool> SampleCheckbox { get; private set; }
        public static ConfigEntry<SampleEnum> SampleCarousel { get; private set; }
        public static ConfigEntry<int> SampleSlider { get; private set; }
        public static ConfigEntry<float> SampleSliderFloat { get; private set; }
        public static ConfigEntry<Color> SampleColor { get; private set; }

        public static void Initialize(ConfigGroup config) {
            SampleCheckbox = config.Bind("Sample Checkbox", false, "Sample checkbox for boolean values.");
            SampleCarousel = config.Bind("Sample Carousel", SampleEnum.Foo, "Sample carousel for enum values.");
            SampleSlider = config.Bind("Sample Slider", 0, "Sample slider for integer values.", new AcceptableValueRange<int>(0, 100));
            SampleSliderFloat = config.Bind("Sample Float Slider", 0f, "Sample slider for floating point values.", new AcceptableValueRange<float>(0, 1));
            SampleColor = config.Bind("Sample Color", new Color(), "Sample color picker.");
        }
    }

    public static class VersionControl {
        public static ConfigEntry<string> VersionOverride;

        public static void Initialize(ConfigGroup config) {
            VersionOverride = config.Bind("Version Override", "", "Input the current build version or '*' to override the version check.");
        }
    }

    public static void Initialize(ConfigFile config) {
        Samples.Initialize(new(config, "Samples"));
        VersionControl.Initialize(new(config, "Version Control"));
    }
}
