using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace RiftOfTheNecroManager;


[BepInPlugin(GUID, "RiftOfTheNecroManager", "0.2.9")]
[NecroManagerInfo(menuNameOverride: NAME)]
internal partial class Plugin : RiftPluginInternal {
    public const string GUID = "com.lalabuff.necrodancer.necromanager";
    public const string NAME = "Rift of the NecroManager";
    
    private Dictionary<string, RiftPluginInternal> LoadedPlugins { get; } = [];
    
    private Plugin() {
        LoadedPlugins[GUID] = this;
        OnPluginLoaded += plugin => {
            LoadedPlugins[plugin.Metadata.GUID] = plugin;
        };
        Log.Message($"{NAME} is initializing...");
        LoadAllMods(); // fire and forget
    }
    
    private async void LoadAllMods() {
        var modInfo = await QueryModInfo();
        foreach(var (guid, info) in modInfo.mods) {
            if(!LoadedPlugins.TryGetValue(guid, out var plugin)) {
                continue;
            }
            
            plugin.PerformVersionCheck(info.compatible, info.updateAvailable);
            
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
        var tcs = new TaskCompletionSource<JsonServerResponse>();
        
        await GlobalTimer.NextTick(); // wait for other plugins to load
        
        var data = new Dictionary<string, object>();
        var gameVersion = Util.GameVersion;
        data["version"] = gameVersion;
        
        var modsData = new Dictionary<string, string>();
        data["mods"] = modsData;
        
        foreach(var plugin in Chainloader.PluginInfos.Values) {
            modsData[plugin.Metadata.GUID] = plugin.Metadata.Version.ToString();
        }
        
        Util.SendJsonRequest("https://necrodancer.lalabuff.com/necromanager", data, request => {
            try {
                if(request.result == UnityWebRequest.Result.Success) {
                    Log.Message(request.downloadHandler.text);
                    var result = JsonConvert.DeserializeObject<JsonServerResponse>(request.downloadHandler.text);
                    tcs.TrySetResult(result);
                    Log.Message($"Successfully retrieved mod data from the {NAME} server.");
                    foreach(var (modId, modInfo) in result.mods) {
                        Log.Message($"Mod {modId}: Compatible={modInfo.compatible}, UpdateAvailable={modInfo.updateAvailable}");
                    }
                    return;
                }
                Log.Error($"Failed to retrieve mod data from the {NAME} server. (Status: {request.responseCode})");
                tcs.TrySetResult(new());
            }
            catch(Exception e) {
                Log.Error($"Error occurred while retrieving mod data from the {NAME} server:");
                Log.Error(e);
                tcs.TrySetResult(new());
            }
            finally {
                request.Dispose();
            }
        });
        
        return await tcs.Task;
    }
}
