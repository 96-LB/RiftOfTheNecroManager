
namespace RiftOfTheNecroManager;

public enum VersionControlOption {
    Automatic,
    Manual,
    Disabled
}


public static class Config {
    public static class VersionControl {
        const string GROUP = "Version Control";
        
        public static Setting<VersionControlOption> AutomaticVersionControl { get; } = new(GROUP, "Automatic Version Control", VersionControlOption.Automatic, $"{Setting.WARNING} Version control prevents bugs and crashes by disabling incompatible mods. Stick to automatic unless you know what you're doing!");
    }
}
