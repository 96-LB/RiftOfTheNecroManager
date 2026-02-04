using FMOD;
using FMODUnity;

namespace RiftOfTheNecroManager;


public static class Sfx {
    public static EventReference ByGuid(string guid) => new() { Guid = GUID.Parse(guid) };
    
    /// <summary>
    /// Sound effect played when a character is added in the virtual keyboard.
    /// </summary>
    public static EventReference AddCharacter { get; } = ByGuid("ce151aa2-1014-4ba6-b9ef-261b30483a6a");
    
    /// <summary>
    /// Sound effect played when a character is removed in the virtual keyboard.
    /// </summary>
    public static EventReference RemoveCharacter { get; } = ByGuid("2abe6639-17ee-4876-8019-a027755d60a7");
    
    /// <summary>
    /// Sound effect played when a submission is confirmed.
    /// </summary>
    public static EventReference Confirm { get; } = ByGuid("29290ef3-9f14-4ff8-8c97-3fd61d8eb90b");
    
    /// <summary>
    /// Sound effect played when a submission is cancelled.
    /// </summary>
    public static EventReference Cancel { get; } = ByGuid("e8b6ddeb-1a32-49c2-8106-114f97ff7d3c");
}
