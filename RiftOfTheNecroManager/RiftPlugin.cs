using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RiftOfTheNecroManager.BeatmapEvents;
using RiftOfTheNecroManager.Patches;
using Shared;
using UnityEngine;

namespace RiftOfTheNecroManager;


public abstract class RiftPluginInternal : BaseUnityPlugin {
    public RiftPluginInfo Metadata => RiftPluginInfo.Of(Info);
    internal Assembly Assembly => GetType().Assembly;
    protected Harmony Harmony { get; }
    
    
    private static bool IsBugSplatDisabled { get; set; } = false;
    private static Harmony BugsplatPatcher { get; } = new("BUGSPLAT"); 
    private protected static event Action<RiftPluginInternal>? OnPluginLoaded;
    private protected static event Action<RiftPluginInternal>? OnPluginUnloaded;
    
    private protected RiftPluginInternal() {
        // private protected prevents direct inheritance outside this assembly
        // do not change access modifier
        DisableBugSplat(); // make sure this runs before anything else happens in case necromanager fails to initialize!
        gameObject.hideFlags = HideFlags.HideAndDontSave; // this prevents the BepInEx manager object from getting destroyed by the game
        PluginData.RegisterAssembly(Assembly, this); // might throw exception!
        Log.RegisterAssembly(Assembly, Logger);
        OnPluginLoaded?.Invoke(this);
        Harmony = new(Metadata.GUID);
    }
    
    private static void DisableBugSplat() {
        if(IsBugSplatDisabled) {
            return;
        }
        
        IsBugSplatDisabled = true;
        var original = typeof(BugSplatAccessor).GetMethod(nameof(BugSplatAccessor.Start), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var prefix = typeof(BugsplatPatch).GetMethod(nameof(BugsplatPatch.Start));
        BugsplatPatcher.Patch(original, prefix: new(prefix));
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
            Metadata.Incompatible = true;
            
            var overrideVersionCheck = Config.Bind("Version Control", "Override Version Check", false, $"{Setting.Warning} Turning this on may cause bugs or crashes when the game updates.");
            var allowedVersions = Config.Bind("Version Control", "Allowed Versions", "0.0.0,0.0.1", $"List of game versions on which to enable the mod when version check is overriden, separated by commas.");
            var versionList = allowedVersions.Value.Split(',').Select(x => x.Trim());
            if(!overrideVersionCheck.Value || !versionList.Contains(Util.GameVersion)) {
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
        if(PluginData.GetPlugin(Assembly) != this) {
            Log.Error("Tried to initialize plugin that is not registered. If this happens, something is very wrong.");
            return;
        }
        
        try {
            Harmony.PatchAll(Assembly);
            Setting.RegisterAssembly(Assembly, Config);
            CustomEvent.RegisterAssembly(Assembly, Metadata.GetCustomEventsName());
            OnInit();
        } catch(Exception e) {
            DeactivateForError(e);
        }
    }
    
    internal void OnDestroy() {
        Harmony.UnpatchSelf();
        if(PluginData.GetPlugin(Assembly) == this) {
            PluginData.UnregisterAssembly(Assembly);
            Log.UnregisterAssembly(Assembly);
            Setting.UnregisterAssembly(Assembly);
            CustomEvent.UnregisterAssembly(Assembly);
            OnPluginUnloaded?.Invoke(this);
        }
        OnUnload();
    }
    
    protected virtual void OnInit() { }
    
    protected virtual void OnUnload() { }
}


[BepInDependency(Plugin.GUID)]
public abstract class RiftPlugin : RiftPluginInternal {
}
