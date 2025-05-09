using HarmonyLib;
using RiftOfTheNecroManager.Scripts;
using Shared.MenuOptions;
using Shared.Title;
using UnityEngine;

namespace RiftOfTheNecroManager.Patches;


[HarmonyPatch(typeof(SettingsMenuManager), nameof(SettingsMenuManager.Start))]
internal static class SettingsMenuManagerPatch {
    public static void Postfix(SettingsMenuManager __instance) {
        // we load a bunch of objects from the accessibility menu and store them as prefabs
        // we clone these to create our own menus
        var template = __instance._riftAccessibilitySettingsController;
        if(template) {
            RiftModsSettingsController.LoadPrefabs(
                template,
                __instance._accessibilityButton,
                template.GetComponentInChildren<ToggleOption>(),
                template.GetComponentInChildren<CarouselOptionGroup>(),
                template._backgroundDetailCarouselOptionPrefab,
                __instance._riftAudioSettingsController._sliderPrefab,
                template._cancelButton
            );
        }
        // create the mods menu controller and populate it
        var controller = RiftModsSettingsController.Create();
        controller.AddAllModMenus();

        // add a button to the base settings menu
        var modsButton = Object.Instantiate(__instance._accessibilityButton, __instance._accessibilityButton.transform.parent);
        modsButton.name = "TextButton - Mods";
        modsButton.OnSubmit += () => {
            __instance._contentParent.SetActive(false);
            controller.gameObject.SetActive(true);
        };
        controller.OnClose += () => {
            if(!__instance.enabled) return;
            controller.ScheduleForNextFrame(() => {
                controller.gameObject.SetActive(false);
                __instance._contentParent.SetActive(true);
            });
        };

        foreach(var label in modsButton._textLabels) {
            Util.ForceSetText(label, "MODS");
        }
        
        // make it a different color than the other buttons
        var color = new Color(196f / 255, 241f / 255, 65f / 255);
        modsButton._selectedTextColor = color;
        modsButton._unselectedTextColor = color.RGBMultiplied(0.5f);

        // add the button to the input controller and layout group as the penultimate option (before BACK)
        __instance._inputController.TryAddOption(modsButton, __instance._inputController.LastOptionIndex);
        var index = modsButton.transform.GetSiblingIndex();
        if(index > 0) {
            modsButton.transform.SetSiblingIndex(index - 1);
        }
        
        Plugin.Log.LogInfo("Successfully created mod settings menu.");
    }
}
