using BepInEx;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, "RiftOfTheNecroManager", "0.2.8")]
public class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public override string AllowedVersions => "1.10.0 1.8.0 1.7.1 1.7.0 1.6.0 1.5.1 1.5.0 1.4.0";
}
