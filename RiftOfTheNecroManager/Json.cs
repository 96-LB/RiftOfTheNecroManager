using System.Collections.Generic;
using Newtonsoft.Json;

namespace RiftOfTheNecroManager;


#pragma warning disable CS0649 // these fields are set via JSON deserialization

[JsonObject(MemberSerialization.Fields)]
internal readonly struct JsonModInfo {
    public readonly bool compatible;
    
    [JsonProperty("update_available")]
    public readonly bool updateAvailable;
    
    public readonly string version;
}


[JsonObject(MemberSerialization.Fields)]
internal readonly struct JsonServerResponse {
    public readonly string version;
    public readonly Dictionary<string, JsonModInfo>? mods;
}
