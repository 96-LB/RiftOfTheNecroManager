using UnityEngine;

namespace RiftOfTheNecroManager;


public record ColorText(Color Color) {
    public string Text(string text) => $"<color={this}>{text}</color>";
    
    public override string ToString() => "#" + ColorUtility.ToHtmlStringRGBA(Color);
    
    public static implicit operator Color(ColorText ct) => ct.Color;
    
    public static implicit operator ColorText(Color ct) => new(ct);
    
    public static ColorText Red { get; } = new Color(0.945f, 0.255f, 0.427f);
    public static ColorText Orange { get; } = new Color(0.945f, 0.427f, 0.255f);
    public static ColorText Green { get; } = new Color(0.427f, 0.945f, 0.255f);
    public static ColorText Blue { get; } = new Color(0.255f, 0.427f, 0.945f);
    public static ColorText Clear { get; } = new Color(0, 0, 0, 0);
}
