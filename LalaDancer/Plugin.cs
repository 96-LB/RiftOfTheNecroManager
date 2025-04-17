using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace LalaDancer;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
    const string GUID = "com.lalabuff.necrodancer.laladancer";
    const string NAME = "LalaDancer";
    const string VERSION = "0.0.1";

    internal static ManualLogSource Log;

    internal void Awake() {
        Log = Logger;

        Harmony harmony = new(GUID);
        harmony.PatchAll();
        
        LalaDancer.Config.Initialize(Config);

        Log.LogInfo($"{NAME} v{VERSION} ({GUID}) has been loaded! Have fun!");
        foreach(var x in harmony.GetPatchedMethods()) {
            Log.LogInfo($"Patched {x}.");
        }
    }
}
