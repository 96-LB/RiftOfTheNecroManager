
using System;

namespace RiftOfTheNecroManager;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class NecroManagerInfoAttribute(string menuNameOverride = "", string customEventsNameOverride = "") : Attribute {
    public string MenuNameOverride { get; } = menuNameOverride;
    public string CustomEventsNameOverride { get; } = customEventsNameOverride;
    
    public static NecroManagerInfoAttribute GetAttribute(Type type) {
        var attr = (NecroManagerInfoAttribute?)GetCustomAttribute(type, typeof(NecroManagerInfoAttribute));
        return attr ?? new();
    }
}
