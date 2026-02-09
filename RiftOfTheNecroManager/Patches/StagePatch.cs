using HarmonyLib;
using RhythmRift;
using Shared.Utilities;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace RiftOfTheNecroManager.Patches;


public class StageState : State<RRStageController, StageState> {
    public BeatmapState Beatmap => BeatmapState.Of(Instance.BeatmapPlayer);
    
    public float StartBeat => Mathf.Max(0,
        Instance._isPracticeMode
        ? Instance._practiceModeStartBeatNumber - Instance._practiceModeTotalBeatsSkippedBeforeStartBeatmap - Instance._microRiftMusicFadeInDurationInBeats
        : 0
    );
    
    public float EndBeat => Instance._isPracticeMode
        ? Instance._practiceModeEndBeatNumber
        : float.MaxValue;
    
    public async Task Preload() {
        Beatmap.Stage = this;
        await Beatmap.Preload(Instance._beatmaps);
    }
}


[HarmonyPatch(typeof(RRStageController))]
public static class StagePatch {
    [HarmonyPatch(nameof(RRStageController.StageInitialize))]
    [HarmonyPostfix]
    public static void StageInitialize(RRStageController __instance, ref IEnumerator __result) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();
        
        IEnumerator Wrapper() {
            yield return original;
            
            var state = StageState.Of(__instance);
            yield return AsyncUtils.WaitForTask(state.Preload());
        }
    }
}
