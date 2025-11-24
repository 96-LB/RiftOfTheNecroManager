using BepInEx;
using BepInEx.Logging;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public const string NAME = "RiftOfTheNecroManager";
    public const string VERSION = "0.2.8";

    public override string AllowedVersions => "1.10.0 1.8.0 1.7.1 1.7.0 1.6.0 1.5.1 1.5.0 1.4.0";

    internal static ManualLogSource Log { get; private set; } = new(NAME);

    protected override void Initialize() {
        Log = Logger;
        RiftOfTheNecroManager.Config.Bind(Config);
        base.Initialize();
    }
}
