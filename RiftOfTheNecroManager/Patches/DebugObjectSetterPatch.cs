using BepInEx.Bootstrap;
using HarmonyLib;
using Shared;
using TMPro;

namespace RiftOfTheNecroManager.Patches;


[HarmonyPatch(typeof(DebugObjectSetter))]
public static class DebugObjectSetterPatch {
    [HarmonyPatch(nameof(DebugObjectSetter.Start))]
    [HarmonyPostfix]
    public static void Start(DebugObjectSetter __instance) {
        foreach(var obj in __instance._objectsToSet) {
            var text = obj.GetComponent<TextMeshProUGUI>();
            if(!text) {
                continue;
            }
            
            var numMods = 0;
            var numUpdates = 0;
            foreach(var plugin in Chainloader.PluginInfos.Values) {
                var info = RiftPluginInfo.Of(plugin);
                if(!info.Deactivated) numMods++;
                if(info.UpdateAvailable) numUpdates++;
            }
            
            text.enableAutoSizing = false;
            text.alignment = TextAlignmentOptions.TopRight;
            text.text = ColorText.Green.Text($"{numMods} MOD{(numMods != 1 ? "S" : "")} ACTIVE");
            text.enableWordWrapping = false;
            if(numUpdates > 0) {
                text.text += ColorText.Blue.Text($"\n{numUpdates} UPDATE{(numUpdates != 1 ? "S" : "")} AVAILABLE!");
            }
            
            if(PluginData.Metadata.UpdateAvailable) {
                text.text += ColorText.Red.Text($"\n<size=1.8em>PLEASE UPDATE RIFT OF THE NECROMANAGER OR YOUR MODS MAY STOP WORKING SOON!");
                text.text += ColorText.Clear.Text($"<size=1em>  im thinking miku miku oo ee oo  ");
            }

            obj.SetActive(true);
        }
    }
}
