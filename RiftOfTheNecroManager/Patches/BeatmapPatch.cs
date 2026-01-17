using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager.BeatmapEvents;
using Shared.RhythmEngine;

namespace RiftOfTheNecroManager.Patches;


public class BeatmapState : State<RRBeatmapPlayer, BeatmapState> {
    public StageState? Stage { get; internal set; }
    public CustomEvent[] CustomEvents { get; private set; } = [];
    public Dictionary<BeatmapEvent, CustomEvent> UnprocessedEvents { get; } = [];
    
    public async Task Preload(IEnumerable<Beatmap> beatmaps) {
        if(Stage == null) {
            Log.Error("Tried to preload beatmap events without a valid stage state!");
            return;
        }
        
        var tasks = new List<Task>();
        CustomEvents = [.. CustomEvent.Enumerate(beatmaps).OrderBy(e => e.Beat)];
        foreach(var customEvent in CustomEvents) {
            if(customEvent.ShouldPreload(Stage)) {
                tasks.Add(customEvent.Preload(Stage));
            }
        }
        
        await Task.WhenAll(tasks);
        
        foreach(var customEvent in CustomEvents) {
            if(customEvent.ShouldSkip(Stage)) {
                customEvent.Skip(Stage);
            } else {
                UnprocessedEvents[customEvent.BeatmapEvent] = customEvent;
            }
        }
    }
    
    public void ProcessBeatEvent(BeatmapEvent beatEvent) {
        if(Stage == null) {
            return;
        }
        
        if(UnprocessedEvents.TryGetValue(beatEvent, out var customEvent)) {
            customEvent.Process(Stage);
            UnprocessedEvents.Remove(beatEvent);
        }
    }
    
}


[HarmonyPatch(typeof(RRBeatmapPlayer))]
public static class BeatmapPatch {
    [HarmonyPatch(nameof(RRBeatmapPlayer.ProcessBeatEvent))]
    [HarmonyPostfix]
    public static void ProcessBeatEvent(RRBeatmapPlayer __instance, BeatmapEvent beatEvent) {
        var state = BeatmapState.Of(__instance);
        state.ProcessBeatEvent(beatEvent);
    }
}
