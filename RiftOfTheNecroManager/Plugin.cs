using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
    const string GUID = "com.lalabuff.necrodancer.necromanager";
    const string NAME = "RiftOfTheNecroManager";
    const string VERSION = "0.0.1";

    internal static ManualLogSource Log;

    internal void Awake() {
        Log = Logger;

        Harmony harmony = new(GUID);
        harmony.PatchAll();
        
        RiftOfTheNecroManager.Config.Initialize(Config);

        Log.LogInfo($"{NAME} v{VERSION} ({GUID}) has been loaded! Have fun!");
        foreach(var x in harmony.GetPatchedMethods()) {
            Log.LogInfo($"Patched {x}.");
        }
    }
}
