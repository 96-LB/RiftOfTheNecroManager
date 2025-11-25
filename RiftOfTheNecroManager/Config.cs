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
        
        public static Setting<bool> SampleCheckbox { get; } = new(GROUP, "Sample Checkbox", false, "Sample checkbox for boolean values.");
        public static Setting<SampleEnum> SampleCarousel { get; } = new(GROUP, "Sample Carousel", SampleEnum.Foo, "Sample carousel for enum values.");
        public static Setting<int> SampleSlider { get; } = new(GROUP, "Sample Slider", 0, "Sample slider for integer values.", new AcceptableValueRange<int>(0, 100));
        public static Setting<float> SampleSliderFloat { get; } = new(GROUP, "Sample Float Slider", 0f, "Sample slider for floating point values.", new AcceptableValueRange<float>(0, 1));
        public static Setting<Color> SampleColor { get; } = new(GROUP, "Sample Color", new(), "Sample color picker.");
    }
}
