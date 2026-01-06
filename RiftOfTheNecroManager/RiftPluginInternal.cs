using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace RiftOfTheNecroManager;

public abstract partial class RiftPluginInternal : BaseUnityPlugin {
    // contains static functionality used by this assembly
    // see RiftPlugin.cs for functionality shared by all rift plugins
    
    public const string NECROMANAGER_GUID = "com.lalabuff.necrodancer.necromanager";
    public const string NECROMANAGER = "Rift of the NecroManager";
    
    private static Dictionary<string, RiftPluginInternal> LoadedPlugins { get; } = [];
    
    protected internal static async void LoadAllMods() {
        var modInfo = await QueryModInfo();
        foreach(var (guid, info) in modInfo.mods) {
            if(!LoadedPlugins.TryGetValue(guid, out var plugin)) {
                continue;
            }
            
            var metadata = plugin.Metadata;
            var log = plugin.Logger;
            var idString = $"{metadata.Name} v{metadata.Version} ({metadata.GUID})";
            
            // TODO: update available notification, and potentially binding this not in the mod's settings
            if(!info.compatible) {
                var config = plugin.Config.Bind("Version Control", "Disable Version Check", false, "[WARNING] Turning this on may cause bugs or crashes when the game updates.");
                
                if(!config.Value) {
                    metadata.Deactivated = true;
                    log.LogFatal($"{idString} has been deactivated because it is not known to be compatible with the current version of the game ({Util.GameVersion}). Please update the game or the mod to the correct version.");
                    continue;
                } else {
                    log.LogWarning($"{idString} is not known to be compatible with the current version of the game ({Util.GameVersion}), but the version check has been disabled. This may cause bugs or crashes.");
                }
            }
            
            foreach(var dep in plugin.Info.Dependencies) {
                if(!dep.Flags.HasFlag(BepInDependency.DependencyFlags.HardDependency)) {
                    continue;
                }
                
                if(!LoadedPlugins.TryGetValue(dep.DependencyGUID, out var depPlugin)) {
                    continue;
                }
                
                if(depPlugin.Metadata.Deactivated) {
                    metadata.Deactivated = true;
                    log.LogFatal($"{idString} has been deactivated because one of its dependencies ({dep.DependencyGUID}) is deactivated.");
                    break;
                }
            }
            
            if(metadata.Deactivated) {
                continue; // necessary because the previous loop may have deactivated this plugin
            }
            
            try {
                plugin.Initialize();
            } catch(Exception e) {
                metadata.Deactivated = true;
                log.LogFatal($"Encountered error while trying to initialize plugin {idString}:");
                log.LogFatal(e);
                continue;
            }
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
                    Log.Message($"Successfully retrieved mod data from the {NECROMANAGER} server.");
                    foreach(var (modId, modInfo) in result.mods) {
                        Log.Message($"Mod {modId}: Compatible={modInfo.compatible}, UpdateAvailable={modInfo.updateAvailable}");
                    }
                    return;
                }
                Log.Error($"Failed to retrieve mod data from the {NECROMANAGER} server. (Status: {request.responseCode})");
                tcs.TrySetResult(new());
            }
            catch(Exception e) {
                Log.Error($"Error occurred while retrieving mod data from the {NECROMANAGER} server:");
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
