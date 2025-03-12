using System.Runtime.CompilerServices;
using UnityEngine;

namespace LalaDancer;


internal static class DebugUtil {
    internal static void PrintAllComponents(GameObject gameObject) {
        Plugin.Log.LogWarning($"Components of [{gameObject}]:");
        foreach(var component in gameObject.GetComponents<Component>()) {
            Plugin.Log.LogWarning($"  • {component}");
        }
    }

    internal static void PrintAllComponents(Component component) {
        PrintAllComponents(component.gameObject);
    }

    private static void PrintAllChildren(Transform transform, int depth, bool recursive = false) {
        foreach(Transform child in transform) {
            Plugin.Log.LogWarning($"{new string(' ', depth * 2)}• {child}");
            if(recursive) {
                PrintAllChildren(child, depth + 1, recursive);
            }
        }
    }   
    
    internal static void PrintAllChildren(Transform transform, bool recursive = false) {
        Plugin.Log.LogWarning($"Children of [{transform}]:");
        PrintAllChildren(transform, 1, recursive);
    }

    internal static void PrintAllChildren(GameObject gameObject, bool recursive = false) {
        PrintAllChildren(gameObject.transform, recursive);
    }

    internal static void PrintAllChildren(Component component, bool recursive = false) {
        PrintAllChildren(component.gameObject, recursive);
    }
}
