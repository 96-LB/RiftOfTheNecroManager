namespace RiftOfTheNecroManager.BeatmapEvents;


/// <summary>
/// Defines the mode for matching custom event types to beatmap event types.
/// </summary>
public enum EventTypeMatchMode {
    /// <summary>
    /// Matches if the custom event type is a subtype of the beatmap event type,
    /// i.e., the beatmap event type is a suffix of the custom event type.
    /// </summary>
    /// <remarks>
    /// A beatmap with type "Base.Super" will match a custom event with type "Base.Super" or "Derived.Base.Super", but not "Super".
    /// </remarks>
    Covariant,
    
    /// <summary>
    /// Matches only if the custom event type exactly matches the beatmap event type.
    /// </summary>
    /// <remarks>
    /// A beatmap with type "Base.Super" will match only a custom event with type "Base.Super".
    /// </remarks>
    Invariant,
    
    /// <summary>
    /// Matches if the custom event type is a supertype of the beatmap event type,
    /// i.e., the custom event type is a suffix of the beatmap event type.
    /// </summary>
    /// <remarks>
    /// A beatmap with type "Base.Super" will match a custom event with type "Base.Super" or "Super", but not "Derived.Base.Super".
    /// </remarks>
    Contravariant,
    
    /// <summary>
    /// Matches only if the custom event type is a proper supertype of the beatmap event type,
    /// i.e., the custom event type is a proper suffix of the beatmap event type and.
    /// </summary>
    /// <remarks>
    /// A beatmap with type "Base.Super" will match only a custom event with type "Super".
    /// </remarks>
    ContravariantStrict
}
