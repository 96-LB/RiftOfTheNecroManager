using BepInEx;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, "RiftOfTheNecroManager", "0.2.9")]
public class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public override string AllowedVersions => "1.11.1";
}
