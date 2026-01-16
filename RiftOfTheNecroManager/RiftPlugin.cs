using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace RiftOfTheNecroManager;


public abstract partial class RiftPluginInternal : BaseUnityPlugin {
    internal Assembly Assembly => GetType().Assembly;
    public RiftPluginInfo Metadata => RiftPluginInfo.Of(Info);
    
    private protected static event Action<RiftPluginInternal>? OnPluginLoaded;
    
    private protected RiftPluginInternal() {
        // private protected prevents direct inheritance outside this assembly
        // do not change access modifier
        Log.SetLog(Assembly, Logger);
        OnPluginLoaded?.Invoke(this);
    }
    
    internal void PerformVersionCheck(string version, bool compatible, bool updateAvailable) {
        if(version != Metadata.Version) {
            Logger.LogWarning($"{Metadata.InfoString} has a version mismatch in the mod info! This should only occur if you updated the mod and the mod info is being loaded from an out-of-date cache. Mod info version: {version}, actual version: {Metadata.Version}");
            compatible = false;
        }
        
        if(!compatible) {
            var config = Config.Bind("Version Control", "Disable Version Check", false, "<color=#f1416d>[WARNING]</color> Turning this on may cause bugs or crashes when the game updates.");
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
            Metadata.UpdateAvailable = true;
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
