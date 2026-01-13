using UnityEngine;

namespace RiftOfTheNecroManager;


public class Palette(byte r, byte g, byte b) {
    
    public static Palette Green { get; } = new(109, 241, 65);
    public static Palette Red { get; } = new(241, 65, 109);
    public static Palette Orange { get; } = new(241, 109, 65);
    
    public Color Primary { get; } = new(r / 255f, g / 255f, b / 255f);
    public Color Dark => Primary.RGBMultiplied(0.5f);
}
