using System;
using HarmonyLib;
using RhythmRift;
using RhythmRift.Enemies;
using Shared.RhythmEngine;
using Shared;
using System.Collections.Generic;
using UnityEngine;
using Shared.Audio;
using FMODUnity;

namespace LalaDancer.Patches;

using P = RREnemyController;
using Audio = State<string, AudioData>;

internal class AudioData {
    internal EnemyAudioData first;
    internal EnemyAudioData second;
    internal EnemyAudioData special;

    internal ref EnemyAudioData Audio(bool? isFollowUp) {
        if(isFollowUp == null) {
            return ref special; // cursed
        } else if(isFollowUp.Value) {
            return ref second;
        } else {
            return ref first;
        }
    }

    private void UnQueue_Internal(Guid[] guids) {
        foreach(Guid guid in guids) {
            if(guid != Guid.Empty) {
                AudioManager.Instance.StopAudioEvent(guid);
            }
        }
    }

    internal void UnQueue(bool? isFollowUp) {
        ref var audio = ref Audio(isFollowUp);
        UnQueue_Internal([audio.HitAudioId, audio.HitCryAudioId, audio.MissAudioId, audio.AttackAudioId]);
        audio = new();
    }

    internal void Hit() {
        UnQueue_Internal([first.MissAudioId, first.AttackAudioId]);
        first = second;
        second = new();
    }

    internal void SetLane(float laneNumber) {
        
        foreach(Guid guid in new[] {
            first.HitAudioId, first.HitCryAudioId, first.MissAudioId, first.AttackAudioId,
            second.HitAudioId, second.HitCryAudioId, second.MissAudioId, second.AttackAudioId,
            special.HitAudioId, special.HitCryAudioId, special.MissAudioId, special.AttackAudioId
        }) {
            if(guid != Guid.Empty) {
                AudioManager.Instance.SetLaneNumber(guid, laneNumber);
            }
        }
    }
}

internal struct EnemyAudioData {
    public float TargetTrueBeatNumber;
    public Guid HitAudioId;
    public Guid HitCryAudioId;
    public Guid MissAudioId;
    public Guid AttackAudioId;

    public readonly bool HasAnyAudio =>
        HitAudioId != Guid.Empty
        || HitCryAudioId != Guid.Empty
        || MissAudioId != Guid.Empty
        || AttackAudioId != Guid.Empty;

    public readonly bool IsTargetingBeatNumber(float beatNumber) {
        return Mathf.Abs(beatNumber - TargetTrueBeatNumber) < 0.01f;
    }
}


[HarmonyPatch(typeof(P))]
internal static class RREnemyControllerPatch {

    [HarmonyPatch("TryQueueActionRowSounds")]
    [HarmonyPrefix]
    internal static bool TryQueueActionRowSounds(
        ref FmodTimeCapsule fmodTimeCapsule,
        bool ____stopQueueingEnemyAudio,
        List<RREnemy> ____activeEnemies,
        InputRatingsDefinition ____inputRatingsDefinition,
        bool ____isGameOver,
        IRRGridDataAccessor ____tileGridAccessor,
        EventReference ____vibeChainHitEventRef,
        EventReference ____inputHitEventRef,
        EventReference ____enemyMissedEventRef,
        bool ____isPracticeMode,
        bool ____shouldQueueEnemyAttackAudio
    ) {
        if(____isGameOver || ____stopQueueingEnemyAudio || ____activeEnemies.Count < 1) {
            return false;
        }
        
        float mostExtremeBeatProgressForRating = ____inputRatingsDefinition.GetMostExtremeBeatProgressForRating(
            InputRating.Ok,
            fmodTimeCapsule.BeatLengthInSeconds,
            shouldReturnEarliestTime: false
        );
        Plugin.Log.LogInfo($"{fmodTimeCapsule.TrueBeatNumber} {fmodTimeCapsule.Time} {BeatmapPlayerPatch_Internal.GetTime(fmodTimeCapsule.TrueBeatNumber)}");

        foreach(RREnemy activeEnemy in ____activeEnemies) {
            if(!activeEnemy.ShouldQueueSounds() || activeEnemy.IsDying) {
                continue;
            }

            float beat = activeEnemy.NextActionRowTrueBeatNumber;
            if(activeEnemy.IsHoldNote && activeEnemy.IsBeingHeld) {
                beat = activeEnemy.NextUpdateTrueBeatNumber;
                if(
                    beat == activeEnemy.NextActionRowTrueBeatNumber
                    || beat > activeEnemy.GetLastHoldBeatNumber() + 0.05f
                ) {
                    continue;
                }
            }
            if(float.IsInfinity(beat) || fmodTimeCapsule.TrueBeatNumber > beat) {
                continue;
            }

            float delay = BeatmapPlayerPatch_Internal.TimeBetweenBeats(fmodTimeCapsule.TrueBeatNumber, beat);
            float timeUntilLatestInputThreshold = BeatmapPlayerPatch_Internal.TimeBetweenBeats(fmodTimeCapsule.TrueBeatNumber, beat + mostExtremeBeatProgressForRating);

            RREnemyControllerPatch_Internal.TryQueueActionRowSoundsForEnemy(
                activeEnemy, beat, delay, timeUntilLatestInputThreshold, false,
                ____tileGridAccessor, ____vibeChainHitEventRef, ____inputHitEventRef, ____enemyMissedEventRef, ____isPracticeMode, ____shouldQueueEnemyAttackAudio
             );

            if(activeEnemy.IsExpectingFollowUpAction) {
                beat = activeEnemy.ExpectedFollowUpActionTrueBeatNumber;
                if(float.IsInfinity(beat) || fmodTimeCapsule.TrueBeatNumber > beat) {
                    continue;
                }

                delay = BeatmapPlayerPatch_Internal.TimeBetweenBeats(fmodTimeCapsule.TrueBeatNumber, beat);
                timeUntilLatestInputThreshold = BeatmapPlayerPatch_Internal.TimeBetweenBeats(fmodTimeCapsule.TrueBeatNumber, beat + mostExtremeBeatProgressForRating);
                RREnemyControllerPatch_Internal.TryQueueActionRowSoundsForEnemy(
                    activeEnemy, beat, delay, timeUntilLatestInputThreshold, true,
                    ____tileGridAccessor, ____vibeChainHitEventRef, ____inputHitEventRef, ____enemyMissedEventRef, ____isPracticeMode, ____shouldQueueEnemyAttackAudio
                );
            }
        }
        return false;
    }

    [HarmonyPatch("TryUpdateAudioLaneNumbers")]
    [HarmonyPostfix]
    internal static void TryUpdateAudioLaneNumbers(
        RREnemy enemyToUpdate,
        IRRGridDataAccessor ____tileGridAccessor
    ) {
        float laneNumber = enemyToUpdate.TargetGridPosition.x - Mathf.Floor(____tileGridAccessor.NumColumns / 2f);
        Audio.Of(enemyToUpdate.EnemyId).SetLane(laneNumber);
    }

    [HarmonyPatch("HasAnyQueuedInputHitSounds")]
    [HarmonyPrefix]
    internal static bool HasAnyQueuedInputHitSounds(
        string enemyInstanceId,
        ref bool __result
    ) {
        var audio = Audio.Of(enemyInstanceId);
        __result = audio.first.HitAudioId != Guid.Empty || audio.first.HitCryAudioId != Guid.Empty;
        return false;
    }

    [HarmonyPatch("SetIsGameOver")]
    [HarmonyPostfix]
    internal static void SetIsGameOver(
        bool isGameOver
    ) {
        if(isGameOver) {
            foreach(var (_, audio) in Audio.All()) {
                audio.UnQueue(false);
                audio.UnQueue(true);
                audio.UnQueue(null);
            }
        }
    }

    [HarmonyPatch("TryProcessHitEnemySoundReactions")]
    [HarmonyPrefix]
    internal static bool TryProcessHitEnemySoundReactions(
        RREnemy enemy
    ) {
        var audio = Audio.Of(enemy.EnemyId);

        if(audio.first.HitAudioId == Guid.Empty && audio.first.HitCryAudioId == Guid.Empty) {
            Plugin.Log.LogError($"Enemy {enemy.DisplayName} ({enemy.EnemyId}) was hit before its update but somehow did not have any queued input hit sounds. {enemy}");
        } else {
            AudioManager.Instance.IsAudioEventPlaying(audio.first.HitCryAudioId);
            audio.Hit();
        }

        return false;
    }


    [HarmonyPatch("TryQueueSpecialSounds")]
    [HarmonyPrefix]
    internal static bool TryQueueSpecialSounds(
        ref FmodTimeCapsule fmodTimeCapsule,
        bool ____stopQueueingEnemyAudio,
        List<RREnemy> ____activeEnemies,
        IRRGridDataAccessor ____tileGridAccessor
    ) {
        if(____stopQueueingEnemyAudio) {
            return false;
        }

        AudioManager instance = AudioManager.Instance;
        for(int i = 0; i < ____activeEnemies.Count; i++) {
            RREnemy rREnemy = ____activeEnemies[i];
            ref var audio = ref Audio.Of(rREnemy.EnemyId).special;
            float laneNumber = rREnemy.TargetGridPosition.x - Mathf.Floor(____tileGridAccessor.NumColumns / 2f);
            float nextUpdateTrueBeatNumber = rREnemy.NextUpdateTrueBeatNumber;

            if(
                float.IsInfinity(nextUpdateTrueBeatNumber)
                || fmodTimeCapsule.TrueBeatNumber > nextUpdateTrueBeatNumber
                || audio.IsTargetingBeatNumber(nextUpdateTrueBeatNumber)
                || !rREnemy.SpecialSoundEventRef.HasValue
                || rREnemy.SpecialSoundEventRef.Value.IsNull
            ) {
                continue;
            }

            float delayInSeconds = BeatmapPlayerPatch_Internal.TimeBetweenBeats(fmodTimeCapsule.TrueBeatNumber, nextUpdateTrueBeatNumber);
            audio = new() {
                HitAudioId = RREnemyControllerPatch_Internal.QueueAudioEvent(rREnemy.SpecialSoundEventRef.Value, laneNumber, delayInSeconds, instance),
                TargetTrueBeatNumber = nextUpdateTrueBeatNumber
            };
        }
        return false;
    }

    [HarmonyPatch("HandleCleanSpecialAudioData")]
    [HarmonyPrefix]
    internal static bool HandleCleanSpecialAudioData(
        RREnemy enemy
    ) {
        Audio.Of(enemy.EnemyId).special = new();
        return false;
    }

    [HarmonyPatch("HandleEnemyActionRowSoundQueueingRequest")]
    [HarmonyPrefix]
    internal static bool HandleEnemyActionRowSoundQueueingRequest() {
        return false;
    }
}

internal static class RREnemyControllerPatch_Internal {
    internal static Guid QueueAudioEvent(
        EventReference eventRef,
        float laneNumber,
        float delayInSeconds,
        AudioManager instance,
        bool shouldCache = true
    ) {
        if(!eventRef.IsNull) {
            var guid = instance.PlayAudioEvent(eventRef, 0f, shouldCache, 0u, delayInSeconds);
            instance.SetLaneNumber(guid, laneNumber);
            return guid;
        }
        return Guid.Empty;
    }

    internal static void TryQueueActionRowSoundsForEnemy(
        RREnemy enemy,
        float targetTrueBeatNumber,
        float timeUntilNextBeat,
        float timeUntilLatestInputThreshold,
        bool isFollowUp,
        IRRGridDataAccessor tileGridAccessor,
        EventReference vibeChainHitEventRef,
        EventReference inputHitEventRef,
        EventReference enemyMissedEventRef,
        bool isPracticeMode,
        bool shouldQueueEnemyAttackAudio
    ) {
        var state = Audio.Of(enemy.EnemyId);
        ref var audio = ref state.Audio(isFollowUp);
        if(
            timeUntilNextBeat > (LatencyManager.AudioLatencyOffset - LatencyManager.VideoLatencyOffset) + 0.1f
            || audio.IsTargetingBeatNumber(targetTrueBeatNumber)
        ) {
            return;
        }
        state.UnQueue(isFollowUp);
        
        Plugin.Log.LogInfo($"Queueing for {enemy.DisplayName} ({enemy.EnemyId}) at {targetTrueBeatNumber} with delay {timeUntilNextBeat}");
        AudioManager instance = AudioManager.Instance;
        float laneNumber = (float)(enemy.TargetGridPosition.x - Mathf.Floor(tileGridAccessor.NumColumns / 2f));
        Guid guid = Guid.Empty;
        if(enemy.ShouldPlayHitSoundInActionRow) {
            guid = QueueAudioEvent(enemy.IsPartOfVibeChain ? vibeChainHitEventRef : inputHitEventRef, laneNumber, timeUntilNextBeat, instance);
        }

        EventReference enemyHitCryEventRef = enemy.GetEnemyHitCryEventRef(isFollowUp, targetTrueBeatNumber);
        Guid guid2 = QueueAudioEvent(enemyHitCryEventRef, laneNumber, timeUntilNextBeat, instance, shouldCache: false);

        float delayInSeconds = timeUntilLatestInputThreshold + Time.fixedDeltaTime * 2f;
        Guid guid3 = Guid.Empty;
        if(!isPracticeMode) {
            guid3 = QueueAudioEvent(enemyMissedEventRef, laneNumber, delayInSeconds, instance);
        }

        Guid guid4 = Guid.Empty;
        if(enemy.AttackDamage > 0 && enemy.ShouldPlayAttackSoundInActionRow && shouldQueueEnemyAttackAudio) {
            guid4 = QueueAudioEvent(enemy.AttackSoundEventRef, laneNumber, delayInSeconds, instance);
        }

        audio = new() {
            TargetTrueBeatNumber = targetTrueBeatNumber,
            HitAudioId = guid,
            HitCryAudioId = guid2,
            MissAudioId = guid3,
            AttackAudioId = guid4,
        };
    }
}
