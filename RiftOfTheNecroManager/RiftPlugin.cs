using BepInEx;
using HarmonyLib;
using Shared;
using System;
using System.Linq;

namespace RiftOfTheNecroManager;

public abstract class RiftPluginInternal : BaseUnityPlugin {
    public abstract string AllowedVersions { get; }
    
    /// <summary>
    /// Overrides the spacing of the mod's name when displayed in the mod menu. By default, spaces are added before capital letters in the mod name. Must match the plugin name (case insensitive, ignoring spaces) to be valid.
    /// </summary>
    public virtual string MenuNameManualOverride => "";
    
    public string MenuName { get; private set; } = "[Unknown Mod]";
    
    internal RiftPluginInternal() {
        // prevents direct inheritance outside this assembly
    }

    protected void Awake() {
        var idString = $"{Info.Metadata.Name} v{Info.Metadata.Version} ({Info.Metadata.GUID})";
        
        if(CheckMenuName()) {
            MenuName = string.Join(" ", MenuNameManualOverride.Split(" ", StringSplitOptions.RemoveEmptyEntries));
        } else {
            MenuName = Util.PascalToSpaced(Info.Metadata.Name);
            if(!string.IsNullOrEmpty(MenuNameManualOverride)) {
            Logger.LogWarning($"The menu name override \"{MenuNameManualOverride}\" does not match the plugin name \"{Info.Metadata.Name}\". The override will be ignored.");
            }
        }
        
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
            return;
        }
    }
    
    protected virtual void Initialize() {
        var assembly = GetType().Assembly;
        var harmony = new Harmony(Info.Metadata.GUID);
        harmony.PatchAll(assembly);
        Log.SetLog(assembly, Logger);
        Setting.BindAssembly(Config, assembly);
    }
    
    private bool CheckMenuName() {
        try {
            var metadataName = Info.Metadata.Name.Replace(" ", "");
            var overrideName = MenuNameManualOverride.Replace(" ", "");
            return string.Equals(metadataName, overrideName, StringComparison.InvariantCultureIgnoreCase);
        } catch {
            // in case someone evil makes the override throw an exception
            return false;
        }
    }
}

#pragma warning disable BepInEx001 // we don't want this to be a BepInEx plugin
[BepInDependency(Plugin.GUID)]
public abstract class RiftPlugin : RiftPluginInternal {
}