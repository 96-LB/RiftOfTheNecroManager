using BepInEx.Configuration;

namespace LalaDancer;

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
        public static void Initialize(ConfigGroup config) {
            SampleCheckbox = config.Bind("Sample Checkbox", false, "Sample checkbox for boolean values.");
            SampleCarousel = config.Bind("Sample Carousel", SampleEnum.Foo, "Sample carousel for enum values.");
            SampleSlider = config.Bind("Sample Slider", 0, "Sample slider for integer values.", new AcceptableValueRange<int>(0, 100));
        }
    }


    public static void Initialize(ConfigFile config) {
        Samples.Initialize(new(config, "Samples"));
    }
}
