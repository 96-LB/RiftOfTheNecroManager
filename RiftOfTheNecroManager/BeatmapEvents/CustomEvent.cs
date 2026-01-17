using RiftOfTheNecroManager.Patches;
using Shared.RhythmEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace RiftOfTheNecroManager.BeatmapEvents;


public abstract class CustomEvent {
    private record AssemblyInfo(Assembly Assembly, string Prefix, EventInfo[] Events);
    private record EventInfo(Type Type, CustomEventAttribute Attribute, ConstructorInfo Constructor);
    
    private static List<AssemblyInfo> Assemblies { get; } = [];
    
    internal static void RegisterAssembly(Assembly assembly, string prefix) {
        // TODO: warnings for types which are improperly configured
        
        var types = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(CustomEvent)) && !type.IsAbstract)
            .Select(type => (type, type.GetCustomAttribute<CustomEventAttribute>(), type.GetConstructor([])))
            .Where(tuple => (tuple.Item2?.IsValid() ?? false) && tuple.Item3 != null)
            .Select(tuple => new EventInfo(tuple.type, tuple.Item2, tuple.Item3))
            .ToArray();
            
        Assemblies.Add(new(assembly, prefix, types));
    }
    
    public static bool TryParse(BeatmapEvent beatmapEvent, [MaybeNullWhen(false)] out CustomEvent customEvent) {
        var types = beatmapEvent.type.Split(); // do NOT make lowercase here, since we need to preserve case after matching
        foreach(var casedType in types) {
            var type = casedType.ToLowerInvariant();
            // first check all registered assemblies for matching prefix
            foreach(var assemblyInfo in Assemblies) {
                var prefix = assemblyInfo.Prefix.ToLowerInvariant();
                var strippedType = "";
                if(type.StartsWith(prefix + ".")) {
                    strippedType = casedType[(prefix.Length + 1)..];
                }
                else if(type.StartsWith(prefix + "::")) {
                    strippedType = casedType[(prefix.Length + 2)..];
                }
                else {
                    continue;
                }
                if(TryParse(beatmapEvent, strippedType, assemblyInfo, out customEvent)) {
                    customEvent.Type = casedType; // restore full type
                    return true;
                }
            }
            // then check all assemblies without prefix
            foreach(var assemblyInfo in Assemblies) {
                if(TryParse(beatmapEvent, casedType, assemblyInfo, out customEvent)) {
                    return true;
                }
            }
        }
        customEvent = null;
        return false;
    }
    
    private static bool TryParse(BeatmapEvent beatmapEvent, string type, AssemblyInfo assembly, [MaybeNullWhen(false)] out CustomEvent customEvent) {
        foreach(var eventInfo in assembly.Events) {
            if(eventInfo.Attribute.Matches(type)) {
                customEvent = (CustomEvent)eventInfo.Constructor.Invoke([]); // ensure type is loaded
                customEvent.Valid = true;
                customEvent.BeatmapEvent = beatmapEvent;
                customEvent.Type = type;
                if(customEvent.IsValid()) {
                    return true;
                }
            }
        }
        customEvent = null;
        return false;
    }
    
    public static IEnumerable<CustomEvent> Enumerate(IEnumerable<BeatmapEvent> events) {
        foreach(var beatmapEvent in events) {
            if(TryParse(beatmapEvent, out var customEvent)) {
                yield return customEvent;
            }
        }
    }
    
    public static IEnumerable<CustomEvent> Enumerate(Beatmap beatmap) {
        return Enumerate(beatmap.BeatmapEvents);
    }
    
    public static IEnumerable<CustomEvent> Enumerate(IEnumerable<Beatmap> beatmaps) {
        return beatmaps.SelectMany(Enumerate);
    }
    
    // instance members
    
    public float Beat => (float)BeatmapEvent.startBeatNumber;
    
    private CustomEventFlags? _flags;
    public CustomEventFlags Flags => _flags ??= GetType().GetCustomAttribute<CustomEventAttribute>()?.Flags ?? CustomEventFlags.None;
    
    public bool ShouldSkipBeat0 => (Flags & CustomEventFlags.SkipBeat0) != 0;
    
    public bool Valid { get; private set; }
    public BeatmapEvent BeatmapEvent { get; private set; }
    public string Type { get; private set; } = "";
    
    public string GetString(string key) {
        var value = BeatmapEvent.GetFirstEventDataAsString($"{Type}.{key}");
        if(!string.IsNullOrWhiteSpace(value)) {
            return value;
        }
        return BeatmapEvent.GetFirstEventDataAsString(key);
    }
    
    public bool? GetBool(string key) => bool.TryParse(GetString(key), out var result) ? result : null;
    public int? GetInt(string key) => int.TryParse(GetString(key), out var result) ? result : null;
    public float? GetFloat(string key) => float.TryParse(GetString(key), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) ? result : null;
    public Color? GetColor(string key) => ColorUtility.TryParseHtmlString(GetString(key), out var color) ? color : null;
    
    // abstract members
    
    public virtual bool IsValid() {
        return Type != "";
    }
    
    
    public virtual bool ShouldPreload(StageState stage) => stage.StartBeat <= Beat && Beat <= stage.EndBeat && (!ShouldSkipBeat0 || stage.StartBeat < Beat);
    
    public virtual bool ShouldSkip(StageState stage) => Beat < stage.StartBeat || (ShouldSkipBeat0 && Beat <= stage.StartBeat);
    
    public abstract Task Preload(StageState stage);
    
    public abstract void Process(StageState stage);
    
    public abstract void Skip(StageState stage);
}
