using BepInEx;
using HarmonyLib;
using Shared;
using System;
using System.Linq;

namespace RiftOfTheNecroManager;

public abstract class RiftPluginInternal : BaseUnityPlugin {
    public abstract string AllowedVersions { get; }

    internal RiftPluginInternal() {
        // prevents direct inheritance outside this assembly
    }

    protected void Awake() {
        var idString = $"{Info.Metadata.Name} v{Info.Metadata.Version} ({Info.Metadata.GUID})";
        try {
            var disableVersionCheck = Config.Bind("Version Control", "Disable Version Check", false, "[WARNING] Turning this on may cause bugs or crashes when the game updates.");
            if(!disableVersionCheck.Value) {
                var allowedVersions = AllowedVersions.Split();
                var gameVersion = BuildInfoHelper.Instance.BuildId.Split('-')[0];
                if(!allowedVersions.Contains(gameVersion)) {
                    Logger.LogFatal($"The current version of the game is not compatible with this plugin. Please update the game or the mod to the correct version. The current mod version is v{Info.Metadata.Version} and the current game version is {gameVersion}. Allowed game versions: {string.Join(", ", allowedVersions)}");
                    return;
                }
            }

            Initialize();
            Logger.LogMessage($"{idString} has been loaded!");
        } catch(Exception e) {
            Logger.LogFatal($"Encountered error while trying to initialize plugin {idString}.");
            Logger.LogFatal(e);
        }
    }

    protected virtual void Initialize() {
        var assembly = GetType().Assembly;
        var harmony = new Harmony(Info.Metadata.GUID);
        harmony.PatchAll(assembly);
        Log.SetLog(assembly, Logger);
        Setting.BindAssembly(Config, assembly);
    }
}

[BepInDependency(Plugin.GUID)]
public abstract class RiftPlugin : RiftPluginInternal {
}