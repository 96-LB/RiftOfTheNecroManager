using BepInEx;

namespace RiftOfTheNecroManager;


[BepInPlugin(NECROMANAGER_GUID, "RiftOfTheNecroManager", "0.2.9")]
[NecroManagerInfo(menuNameOverride: NECROMANAGER)]
internal class Plugin : RiftPluginInternal {
    public new void Awake() {
        Logger.LogMessage($"{NECROMANAGER} is initializing...");
        LoadAllMods();
    }
}
