
using System;

namespace RiftOfTheNecroManager.BeatmapEvents;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CustomEventAttribute(string type, CustomEventMatchMode matchMode = CustomEventMatchMode.Covariant, CustomEventFlags flags = CustomEventFlags.None) : Attribute {
    public string Type { get; } = Util.RemoveWhitespace(type);
    public string[] TypeSegments { get; } = Util.RemoveWhitespace(type).ToLowerInvariant().Split('.', StringSplitOptions.RemoveEmptyEntries);
    public CustomEventMatchMode MatchMode { get; } = matchMode;
    public CustomEventFlags Flags { get; } = flags;
    
    public bool ShouldSkipBeat0 => (Flags & CustomEventFlags.SkipBeat0) != 0;
    
    public bool IsValid() => !string.IsNullOrWhiteSpace(Type);
    
    public bool Matches(string type) {
        var eventType = type.ToLowerInvariant().Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        var lengthCheck = MatchMode switch {
            CustomEventMatchMode.Covariant => eventType.Length <= TypeSegments.Length,
            CustomEventMatchMode.Invariant => eventType.Length == TypeSegments.Length,
            CustomEventMatchMode.Contravariant => eventType.Length >= TypeSegments.Length,
            CustomEventMatchMode.ContravariantStrict => eventType.Length > TypeSegments.Length,
            _ => false
        };
        
        if(lengthCheck) {
            return false;
        }
        
        for(int i = 1; i <= Math.Min(eventType.Length, TypeSegments.Length); i++) {
            if(eventType[^i] != TypeSegments[^i]) {
                return false;
            }
        }
        
        return true;
    }
}
