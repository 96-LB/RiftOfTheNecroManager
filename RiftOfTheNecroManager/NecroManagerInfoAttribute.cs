
using System;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class NecroManagerInfoAttribute(string menuNameOverride = "") : Attribute {
    public string MenuNameOverride { get; } = menuNameOverride;
    
    public static NecroManagerInfoAttribute GetAttribute(Type type) {
        var attr = (NecroManagerInfoAttribute?)GetCustomAttribute(type, typeof(NecroManagerInfoAttribute));
        return attr ?? new();
    }
}
