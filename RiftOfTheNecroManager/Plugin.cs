using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, NAME, VERSION)]
[NecroManagerInfo(menuNameOverride: MENU_NAME)]
internal partial class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public const string NAME = "RiftOfTheNecroManager";
    public const string VERSION = "1.0.1";
    public const string MENU_NAME = "Rift of the NecroManager";
    
    private Dictionary<string, RiftPluginInternal> LoadedPlugins { get; } = [];
    
    private Plugin() {
        LoadedPlugins[GUID] = this;
        OnPluginLoaded += plugin => {
            LoadedPlugins[plugin.Metadata.GUID] = plugin;
        };
        Log.Info($"{MENU_NAME} is initializing...");
        RiftOfTheNecroManager.Config.VersionControl.AutomaticVersionControl.Bind(Config);
        LoadAllMods(); // fire and forget
    }
    
    private async void LoadAllMods() {
        var modInfo = await QueryModInfo();
        foreach(var (guid, plugin) in LoadedPlugins) {
            var info = modInfo.mods?.GetValueOrDefault(guid);
            var version = info?.version ?? plugin.Metadata.Version;
            var compatible = info?.compatible ?? false;
            var updateAvailable = info?.updateAvailable ?? false;
            
            plugin.PerformVersionCheck(version, compatible, updateAvailable);
            
            foreach(var dep in plugin.Info.Dependencies) {
                if(!dep.Flags.HasFlag(BepInDependency.DependencyFlags.HardDependency)) {
                    continue;
                }
                
                if(!LoadedPlugins.TryGetValue(dep.DependencyGUID, out var depPlugin)) {
                    continue;
                }
                
                if(depPlugin.Metadata.Deactivated) {
                    plugin.DeactivateForDependency(dep.DependencyGUID);
                    break;
                }
            }
            
            if(plugin.Metadata.Deactivated) {
                continue;
            }
            
            plugin.Initialize();
        }
    }
    
    private static async Task<JsonServerResponse> QueryModInfo() {
        await GlobalTimer.NextTick(); // wait for other plugins to load
        
        var versionControl = RiftOfTheNecroManager.Config.VersionControl.AutomaticVersionControl;
        if(versionControl != VersionControlOption.Automatic) {
            Log.Warning("Automatic version control is disabled. Mod compatibility info might be outdated or incorrect.");
            return versionControl == VersionControlOption.Manual ? LoadFallbackModInfo() : new();
        }
        
        var tcs = new TaskCompletionSource<JsonServerResponse>();
        
        var data = new Dictionary<string, object>();
        var gameVersion = Util.GameVersion;
        data["version"] = gameVersion;
        
        var modsData = new Dictionary<string, string>();
        data["mods"] = modsData;
        
        foreach(var plugin in Chainloader.PluginInfos.Values) {
            var info = RiftPluginInfo.Of(plugin);
            modsData[info.GUID] = info.Version;
        }
        
        Log.Info($"Retrieving mod info from the {MENU_NAME} server...");
        Util.SendJsonRequest("https://necrodancer.lalabuff.com/necromanager", data, request => {
            if(request.result == UnityWebRequest.Result.Success) {
                try {
                    var info = request.downloadHandler.text;
                    var result = JsonConvert.DeserializeObject<JsonServerResponse>(info);
                    CacheResponseInfo(info);
                    Log.Info($"Successfully retrieved mod info from the {MENU_NAME} server.");
                    tcs.TrySetResult(result);
                    return;
                } catch(Exception e) {
                    Log.Error($"Error occurred while retrieving info from the {MENU_NAME} server:");
                    Log.Error(e);
                }
            }
            
            // fallback behavior on failure or error
            Log.Error($"Failed to retrieve mod info from the {MENU_NAME} server. (Status: {request.responseCode})");
            tcs.TrySetResult(LoadFallbackModInfo());
        });
        
        return await tcs.Task;
    }
    
    private static void CacheResponseInfo(string json) {
        var cachePath = Path.Combine(PluginData.DataPath, "mod_info.json");
        Directory.CreateDirectory(PluginData.DataPath);
        try {
            File.WriteAllText(cachePath, json);
        } catch(Exception e) {
            Log.Warning("Failed to cache mod info:");
            Log.Warning(e);
        }
    }
    
    private static JsonServerResponse LoadFallbackModInfo() {
        Log.Info("Attempting to load mod info from cache...");
        
        var cachePath = Path.Combine(PluginData.DataPath, "mod_info.json");
        try {
            if(File.Exists(cachePath)) {
                var json = File.ReadAllText(cachePath);
                var result = JsonConvert.DeserializeObject<JsonServerResponse>(json);
                Log.Info($"Successfully loaded mod info from cache.");
                return result;
            }
        } catch(Exception e) {
            Log.Warning("Failed to load mod info from cache:");
            Log.Warning(e);
        }
        
        Log.Fatal("Mod info could not be loaded. All mods will be assumed to be incompatible.");
        return new();
    }
}
