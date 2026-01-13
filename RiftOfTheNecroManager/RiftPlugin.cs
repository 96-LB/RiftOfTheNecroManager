using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace RiftOfTheNecroManager;

public abstract partial class RiftPluginInternal : BaseUnityPlugin {
    internal Assembly Assembly => GetType().Assembly;
    public RiftPluginInfo Metadata => RiftPluginInfo.Of(Info);
    
    private protected static event Action<RiftPluginInternal>? OnPluginLoaded;
    
    internal RiftPluginInternal() {
        // internal prevents direct inheritance outside this assembly
        // do not change access modifier
        Log.SetLog(Assembly, Logger);
        OnPluginLoaded?.Invoke(this);
    }
    
    internal void PerformVersionCheck(bool compatible, bool updateAvailable) {
        if(!compatible) {
            var config = Config.Bind("Version Control", "Disable Version Check", false, "[WARNING] Turning this on may cause bugs or crashes when the game updates.");
            Metadata.Incompatible = true;
            
            if(!config.Value) {
                Metadata.Deactivated = true;
                Logger.LogFatal($"{Metadata.InfoString} has been deactivated because it is not known to be compatible with the current version of the game ({Util.GameVersion}).");
            } else {
                Logger.LogWarning($"{Metadata.InfoString} is not known to be compatible with the current version of the game ({Util.GameVersion}), but the version check has been disabled. This may cause bugs or crashes.");
            }
        }
        if(updateAvailable) {
            Logger.LogMessage($"An update for this mod is available. Please download the latest version!");
        }
    }
    
    internal void DeactivateForDependency(string dependencyGUID) {
        Metadata.Deactivated = true;
        Logger.LogFatal($"{Metadata.InfoString} has been deactivated because one of its dependencies ({dependencyGUID}) is deactivated.");
    }
    
    internal void DeactivateForError(Exception e) {
        Metadata.Deactivated = true;
        Logger.LogFatal($"Encountered error while trying to initialize plugin {Metadata.InfoString}:");
        Logger.LogFatal(e);
    }
    
    internal void Initialize() {
        try {
            var harmony = new Harmony(Metadata.GUID);
            harmony.PatchAll(Assembly);
            Setting.BindAssembly(Config, Assembly);
            OnInit();
        } catch(Exception e) {
            DeactivateForError(e);
        }
    }
    
    protected virtual void OnInit() { }
}


[BepInDependency(Plugin.GUID)]
public abstract class RiftPlugin : RiftPluginInternal {
}
