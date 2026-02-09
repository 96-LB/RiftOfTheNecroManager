using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RiftOfTheNecroManager.BeatmapEvents;

namespace RiftOfTheNecroManager;


public abstract partial class RiftPluginInternal : BaseUnityPlugin {
    internal Assembly Assembly => GetType().Assembly;
    public RiftPluginInfo Metadata => RiftPluginInfo.Of(Info);
    
    private protected static event Action<RiftPluginInternal>? OnPluginLoaded;
    
    private protected RiftPluginInternal() {
        // private protected prevents direct inheritance outside this assembly
        // do not change access modifier
        PluginData.RegisterAssembly(Assembly, this); // might throw exception!
        Log.RegisterAssembly(Assembly, Logger);
        OnPluginLoaded?.Invoke(this);
    }
    
    internal void PerformVersionCheck(string version, bool compatible, bool updateAvailable) {
        if(RiftOfTheNecroManager.Config.VersionControl.AutomaticVersionControl == VersionControlOption.Disabled) {
            Log.Warning($"Skipping version check for {Metadata.InfoString} because version control is disabled. This may cause bugs or crashes.");
            return;
        }
        
        if(version != Metadata.Version) {
            Log.Warning($"{Metadata.InfoString} has a version mismatch in the mod info! This should only occur if you updated the mod and the mod info is being loaded from an out-of-date cache. Mod info version: {version}, actual version: {Metadata.Version}");
            compatible = false;
        }
        
        if(!compatible) {
            var config = Config.Bind("Version Control", "Disable Version Check", false, $"{Setting.WARNING} Turning this on may cause bugs or crashes when the game updates.");
            Metadata.Incompatible = true;
            
            if(!config.Value) {
                Metadata.Deactivated = true;
                Log.Fatal($"{Metadata.InfoString} has been deactivated because it is not known to be compatible with the current version of the game ({Util.GameVersion}).");
            } else {
                Log.Warning($"{Metadata.InfoString} is not known to be compatible with the current version of the game ({Util.GameVersion}), but the version check has been disabled. This may cause bugs or crashes.");
            }
        }
        if(updateAvailable) {
            Log.Message($"An update for {Metadata.InfoString} is available. Please download the latest version!");
            Metadata.UpdateAvailable = true;
        }
    }
    
    internal void DeactivateForDependency(string dependencyGUID) {
        Metadata.Deactivated = true;
        Log.Fatal($"{Metadata.InfoString} has been deactivated because one of its dependencies ({dependencyGUID}) is deactivated.");
    }
    
    internal void DeactivateForError(Exception e) {
        Metadata.Deactivated = true;
        Log.Fatal($"Encountered error while trying to initialize plugin {Metadata.InfoString}:");
        Log.Fatal(e);
    }
    
    internal void Initialize() {
        try {
            var harmony = new Harmony(Metadata.GUID);
            harmony.PatchAll(Assembly);
            Setting.RegisterAssembly(Assembly, Config);
            CustomEvent.RegisterAssembly(Assembly, Metadata.GetCustomEventsName());
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
