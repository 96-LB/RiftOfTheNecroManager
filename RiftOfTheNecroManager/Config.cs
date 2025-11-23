using BepInEx.Configuration;
using UnityEngine;

namespace RiftOfTheNecroManager;

public enum SampleEnum {
    Foo,
    Bar,
    Baz
}

public static class Config {
    public static class Samples {
        const string GROUP = "Samples";
        
        public static Setting<bool> SampleCheckbox { get; } = new("Sample Checkbox", false, "Sample checkbox for boolean values.");
        public static Setting<SampleEnum> SampleCarousel { get; } = new("Sample Carousel", SampleEnum.Foo, "Sample carousel for enum values.");
        public static Setting<int> SampleSlider { get; } = new("Sample Slider", 0, "Sample slider for integer values.", new AcceptableValueRange<int>(0, 100));
        public static Setting<float> SampleSliderFloat { get; } = new("Sample Float Slider", 0f, "Sample slider for floating point values.", new AcceptableValueRange<float>(0, 1));
        public static Setting<Color> SampleColor { get; } = new("Sample Color", new Color(), "Sample color picker.");
        
        public static void Bind(ConfigFile config) {
            SampleCheckbox.Bind(config, GROUP);
            SampleCarousel.Bind(config, GROUP);
            SampleSlider.Bind(config, GROUP);
            SampleSliderFloat.Bind(config, GROUP);
            SampleColor.Bind(config, GROUP);
        }
    }

    public static class VersionControl {
        const string GROUP = "Version Control";

        public static Setting<bool> DisableVersionCheck { get; } = new("Disable Version Check", false, "[WARNING] Turning this on may cause bugs or crashes when the game updates.");

        public static void Bind(ConfigFile config) {
            DisableVersionCheck.Bind(config, GROUP);
        }
    }

    public static void Bind(ConfigFile config) {
        Samples.Bind(config);
        VersionControl.Bind(config);
    }
}
