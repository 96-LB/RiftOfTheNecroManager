using System;

namespace RiftOfTheNecroManager.BeatmapEvents;


[Flags]
public enum CustomEventFlags {
    None = 0,
    SkipBeat0 = 1 << 0,
}
