using BepInEx;

namespace RiftOfTheNecroManager;


[BepInPlugin(RiftPlugin.NECROMANAGER_GUID, "RiftOfTheNecroManager", "0.2.9")]
[NecroManagerInfo(menuNameOverride: "Rift of the NecroManager")]
public class Plugin : RiftPluginInternal {
}
