
using System;

namespace RiftOfTheNecroManager.BeatmapEvents;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CustomEventAttribute(string type, EventTypeMatchMode matchMode = EventTypeMatchMode.Covariant) : Attribute {
    public string Type { get; } = Util.RemoveWhitespace(type);
    public string[] TypeSegments { get; } = Util.RemoveWhitespace(type).ToLowerInvariant().Split('.', StringSplitOptions.RemoveEmptyEntries);
    public EventTypeMatchMode MatchMode { get; } = matchMode;
    
    public bool IsValid() => !string.IsNullOrWhiteSpace(Type);
    
    public bool Matches(string type) {
        var eventType = type.ToLowerInvariant().Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        var lengthCheck = MatchMode switch {
            EventTypeMatchMode.Covariant => eventType.Length <= TypeSegments.Length,
            EventTypeMatchMode.Invariant => eventType.Length == TypeSegments.Length,
            EventTypeMatchMode.Contravariant => eventType.Length >= TypeSegments.Length,
            EventTypeMatchMode.ContravariantStrict => eventType.Length > TypeSegments.Length,
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
