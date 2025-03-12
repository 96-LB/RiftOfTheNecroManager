using BepInEx;
using BepInEx.Configuration;
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

    internal static Sprite QuarterBeat;
    internal static Sprite ThirdBeat;
    internal static Sprite TwoThirdBeat;
    internal static Sprite ThreeQuarterBeat;


    private Sprite MakeSprite(byte[] data) {
        Texture2D tex = new(0, 0);
        tex.LoadImage(data);
        return Sprite.Create(
            tex,
            new(0, 0, tex.width, tex.height),
            new(0.5f, 0.5f),
            48
        );
    }

    internal void Awake() {
        Log = Logger;

        Harmony harmony = new(GUID);
        harmony.PatchAll();
        
        QuarterBeat = MakeSprite(Properties.Resources.QuarterBeat);
        ThirdBeat = MakeSprite(Properties.Resources.ThirdBeat);
        TwoThirdBeat = MakeSprite(Properties.Resources.TwoThirdBeat);
        ThreeQuarterBeat = MakeSprite(Properties.Resources.ThreeQuarterBeat);

        Log.LogInfo($"{NAME} v{VERSION} ({GUID}) has been loaded! Have fun!");
        foreach(var x in harmony.GetPatchedMethods()) {
            Log.LogInfo($"Patched {x}.");
        }
    }
}
