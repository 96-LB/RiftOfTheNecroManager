using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Shared;
using System;
using System.Linq;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
    const string GUID = "com.lalabuff.necrodancer.necromanager";
    const string NAME = "RiftOfTheNecroManager";
    const string VERSION = "0.2.8";

    public const string ALLOWED_VERSIONS = "1.10.0 1.8.0 1.7.1 1.7.0 1.6.0 1.5.1 1.5.0 1.4.0";
    public static string[] AllowedVersions => ALLOWED_VERSIONS.Split(' ');

    internal static ManualLogSource Log { get; private set; } = new(NAME);

    internal void Awake() {
        try {
            Log = Logger;

            RiftOfTheNecroManager.Config.Bind(Config);

            var gameVersion = BuildInfoHelper.Instance.BuildId.Split('-')[0];
            if(!AllowedVersions.Contains(gameVersion) && !RiftOfTheNecroManager.Config.VersionControl.DisableVersionCheck) {
                Log.LogFatal($"The current version of the game is not compatible with this plugin. Please update the game or the mod to the correct version. The current mod version is v{VERSION} and the current game version is {gameVersion}. Allowed game versions: {string.Join(", ", AllowedVersions)}");
                return;
            }

            Harmony harmony = new(GUID);
            harmony.PatchAll();
            Log.LogMessage($"{NAME} v{VERSION} ({GUID}) has been loaded!");
        } catch(Exception e) {
            Log.LogFatal("Encountered error while trying to initialize plugin.");
            Log.LogFatal(e);
        }
    }
}
