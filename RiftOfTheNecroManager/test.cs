using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using Shared;
using UnityEngine.Networking;


namespace RiftOfTheNecroManager;


internal class ModInfos {
    public class ModInfo(bool compatible, bool updateAvailable) {
        public bool Compatible { get; } = compatible;
        public bool UpdateAvailable { get; } = updateAvailable;
    }
    
    public Dictionary<string, ModInfo> Mods { get; } = [];
}


internal static class Util2 {
    public static Task<ModInfos> GetModInfo() {
        var jsonData = new Dictionary<string, object>();
        var gameVersion = BuildInfoHelper.Instance.BuildId.Split('-')[0];
        jsonData["version"] = gameVersion;
        
        var modsData = new Dictionary<string, string>();
        jsonData["mods"] = modsData;
        
        foreach(var plugin in Chainloader.PluginInfos.Values) {
            modsData[plugin.Metadata.GUID] = plugin.Metadata.Version.ToString();
        }
        
        var json = JsonConvert.SerializeObject(jsonData);
        Log.Message(json);
        
        var tcs = new TaskCompletionSource<ModInfos>();
        var request = new UnityWebRequest("https://necrodancer.lalabuff.com/necromanager", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        request.SendWebRequest().completed += delegate {
            try {
                if(request.result == UnityWebRequest.Result.Success) {
                    Log.Message(request.downloadHandler.text);
                    var result = JsonConvert.DeserializeObject<ModInfos>(request.downloadHandler.text);
                    if(result != null) {
                        tcs.TrySetResult(result);
                        Log.Message("Successfully retrieved mod data from RiftOfTheNecroManager server.");
                        foreach(var (modId, modInfo) in result.Mods) {
                            Log.Message($"Mod {modId}: Compatible={modInfo.Compatible}, UpdateAvailable={modInfo.UpdateAvailable}");
                        }
                        return;
                    }
                }
                Log.Error($"Failed to retrieve mod data from RiftOfTheNecroManager server. (Status: {request.responseCode})");
                tcs.TrySetResult(new());
            }
            catch(Exception e) {
                Log.Error("Error occurred while retrieving mod data from RiftOfTheNecroManager server:");
                Log.Error(e);
                tcs.TrySetResult(new());
            }
            finally {
                request.Dispose();
            }
        };
        return tcs.Task;
    }
}
