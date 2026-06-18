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
[NecroManagerInfo(menuNameOverride: MENU_NAME, isBeta: true)]
internal partial class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public const string NAME = "RiftOfTheNecroManager";
    public const string VERSION = "1.1.1";
    public const string MENU_NAME = "Rift of the NecroManager";
    
    public static string CachePath => Path.Combine(PluginData.DataPath, "mod_info.json");
    
    private static Dictionary<string, RiftPluginInternal> LoadedPlugins { get; } = [];
    
    private static JsonServerResponse? ModInfo { get; set; } = null;
    
    private Plugin() {
        Log.Info($"{MENU_NAME} is initializing...");
        LoadedPlugins[GUID] = this;
        
        OnPluginLoaded += plugin => {
            LoadedPlugins[plugin.Metadata.GUID] = plugin;
            
            if(ModInfo is not null) {
                // mod compatibility has already been queried
                Util.ScheduleForNextFrame(this, () => LoadModFromCache(plugin));
            }
        };
        
        OnPluginUnloaded += plugin => {
            LoadedPlugins.Remove(plugin.Metadata.GUID);
        };
        
        RiftOfTheNecroManager.Config.VersionControl.AutomaticVersionControl.Bind(Config);
        LoadAllMods(); // fire and forget
    }
    
    private async void LoadAllMods() {
        ModInfo = await QueryModInfo(); // updates the cache
        foreach(var (_, plugin) in LoadedPlugins) {
            LoadModFromCache(plugin);
        }
    }
    
    private void LoadModFromCache(RiftPluginInternal plugin) {
        var info = ModInfo?.mods?.GetValueOrDefault(plugin.Metadata.GUID);
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
        
        if(!plugin.Metadata.Deactivated) {
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
        
        Log.Info($"Retrieving mod compatibility from the {MENU_NAME} server...");
        Util.SendJsonRequest("https://necrodancer.lalabuff.com/necromanager", data, request => {
            if(request.result == UnityWebRequest.Result.Success) {
                try {
                    var info = request.downloadHandler.text;
                    var result = JsonConvert.DeserializeObject<JsonServerResponse>(info);
                    Log.Info($"Successfully retrieved mod compatibility from the {MENU_NAME} server.");
                    result = CacheResponseInfo(result);
                    tcs.TrySetResult(result);
                    return;
                } catch(Exception e) {
                    Log.Error($"Error occurred while retrieving mod compatibility from the {MENU_NAME} server:");
                    Log.Error(e);
                }
            }
            
            // fallback behavior on failure or error
            Log.Error($"Failed to retrieve mod compatibility from the {MENU_NAME} server. (Status: {request.responseCode})");
            tcs.TrySetResult(LoadFallbackModInfo());
        });
        
        return await tcs.Task;
    }
    
    private static JsonServerResponse CacheResponseInfo(JsonServerResponse response) {
        Log.Info("Updating mod compatibility cache...");
        
        var cache = LoadFallbackModInfo();
        if(cache.version == response.version && cache.mods is not null) {
            foreach(var x in response.mods ?? []) {
                cache.mods[x.Key] = x.Value;
            }
        } else {
            Log.Info("The mod compatibility cache is outdated or invalid and will be replaced.");
            cache = response;
        }
        
        Directory.CreateDirectory(PluginData.DataPath);
        try {
            File.WriteAllText(CachePath, JsonConvert.SerializeObject(cache));
            Log.Info("Successfully updated mod compatibility cache.");
        } catch(Exception e) {
            Log.Error("Failed to cache mod compatibility:");
            Log.Error(e);
        }
        
        return cache;
    }
    
    private static JsonServerResponse LoadFallbackModInfo() {
        // TODO: this can be refactored in a way which merges it with CacheResponseInfo, since this has the same output as CacheResponseInfo(new())
        Log.Info("Attempting to load mod compatibility from cache...");
        
        try {
            if(File.Exists(CachePath)) {
                var json = File.ReadAllText(CachePath);
                var result = JsonConvert.DeserializeObject<JsonServerResponse>(json);
                Log.Info($"Successfully loaded mod compatibility from cache.");
                return result;
            }
        } catch(Exception e) {
            Log.Warning("Failed to load mod compatibility from cache:");
            Log.Warning(e);
        }
        
        Log.Fatal("Mod compatibility could not be loaded. All mods will be assumed to be incompatible.");
        return new();
    }
}
