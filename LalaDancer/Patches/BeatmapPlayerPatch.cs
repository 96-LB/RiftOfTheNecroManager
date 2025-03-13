using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using System;
using System.Linq;
using UnityEngine;

namespace LalaDancer.Patches;

using P = BeatmapPlayer;


[HarmonyPatch(typeof(P), "SetBeatmapInternal")]
internal static class BeatmapPlayerPatch {
    internal static void Postfix(
        Beatmap beatmapToSet,
        float ____activeSpeedAdjustment
    ) {
        BeatmapPlayerPatch_Internal.beatmap = beatmapToSet;
        BeatmapPlayerPatch_Internal.speedAdjustment = ____activeSpeedAdjustment;
        Plugin.Log.LogInfo($"Set beatmap to {beatmapToSet.name} with speed adjustment {____activeSpeedAdjustment}");
    }
}

internal static class BeatmapPlayerPatch_Internal {
    internal static Beatmap beatmap;
    internal static float speedAdjustment = 1f;
    
    internal static float GetTime(float beat) {
        if(beatmap == null) {
            throw new Exception("Tried to get time from beat number, but no beatmap is loaded!");
        }

        // TODO: just patch Beatmap.GetTimeFromBeatNumber; this code is identical except for one part (see comment below)
        // return Beatmap.GetTimeFromBeatNumber(beat);
        beat -= 1;
        if(beat <= 0f) {
            return 0f;
        }

        if(!beatmap.HasBeatTimings) {
            return 60f / Mathf.Max(1, beatmap.bpm) * beat;
        }

        // this is a > in the original message but this causes a crash on the last beat
        // also the +1 on the excess line was a -1 in the original function (also bugged)
        if(beat >= beatmap.BeatTimings.Count - 1) {
            double beatLength = beatmap.BeatTimings[^1] - beatmap.BeatTimings[^2];
            float excess = beat - beatmap.BeatTimings.Count + 1;
            return (float)(beatmap.BeatTimings[^1] + excess * beatLength);
        }

        // this looks different but is the same
        int beatFloor = Mathf.FloorToInt(beat);
        int beatCeil = beatFloor + 1;
        float beatProgress = beat % 1;
        return Mathf.Lerp((float)beatmap.BeatTimings[beatFloor], (float)beatmap.BeatTimings[beatCeil], beatProgress);
    }

    internal static float TimeBetweenBeats(float currentBeat, float targetBeat) {
        return (GetTime(targetBeat) - GetTime(currentBeat)) / speedAdjustment;
    }
}
