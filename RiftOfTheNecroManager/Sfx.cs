using FMOD;
using FMODUnity;
using Shared.Audio;

namespace RiftOfTheNecroManager;


public static class Sfx {
    /// <summary>
    /// Load an event reference by GUID.
    /// </summary>
    /// <param name="guid">The GUID of the event to load.</param>
    /// <returns>The loaded event reference with the provided GUID.</returns>
    public static EventReference ByGuid(string guid) => new() { Guid = GUID.Parse(guid) };
    
    /// <summary>
    /// Immediately play a sound effect.
    /// </summary>
    public static void Play(EventReference sfx) {
        AudioManager.Instance.PlayAudioEvent(sfx, 0f, shouldCache: true, 0u, 0f, shouldApplyLatency: false);
    }
    
    /// <summary>
    /// Sound effect played when a text character is added in the virtual keyboard.
    /// </summary>
    public static EventReference AddCharacter { get; } = ByGuid("ce151aa2-1014-4ba6-b9ef-261b30483a6a");
    
    /// <summary>
    /// Sound effect played when a text character is removed in the virtual keyboard.
    /// </summary>
    public static EventReference RemoveCharacter { get; } = ByGuid("2abe6639-17ee-4876-8019-a027755d60a7");
    
    /// <summary>
    /// Sound effect played when a text submission is confirmed.
    /// </summary>
    public static EventReference Confirm { get; } = ByGuid("29290ef3-9f14-4ff8-8c97-3fd61d8eb90b");
    
    /// <summary>
    /// Sound effect played when a text submission is cancelled.
    /// </summary>
    public static EventReference Cancel { get; } = ByGuid("e8b6ddeb-1a32-49c2-8106-114f97ff7d3c");
    
    /// <summary>
    /// Sound effect played when the back button is pressed in a menu.
    /// </summary>
    public static EventReference Back { get; } = ByGuid("2c4d21b3-d604-452a-b194-942571a9b90c");
}
