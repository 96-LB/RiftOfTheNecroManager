
using System;

namespace RiftOfTheNecroManager;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class NecroManagerInfoAttribute(string menuNameOverride = "", string customEventsNameOverride = "", bool isBeta = false) : Attribute {
    public string MenuNameOverride { get; } = menuNameOverride;
    public string CustomEventsNameOverride { get; } = customEventsNameOverride;
    public bool IsBeta { get; } = isBeta;
    
    public static NecroManagerInfoAttribute GetAttribute(Type type) {
        var attr = (NecroManagerInfoAttribute?)GetCustomAttribute(type, typeof(NecroManagerInfoAttribute));
        return attr ?? new();
    }
}
