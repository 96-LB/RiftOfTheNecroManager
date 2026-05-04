using HarmonyLib;
using Shared;

namespace RiftOfTheNecroManager.Patches;


[HarmonyPatch(typeof(BugSplatAccessor))]
public static class BugsplatPatch {
    [HarmonyPatch(nameof(BugSplatAccessor.Start))]
    [HarmonyPrefix]
    public static bool Start() {
        // prevent initialization of error reporting
        BugSplatAccessor.Instance.BugSplat.ShouldPostException = e => false;
        return false;
    }
}
