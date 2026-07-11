using HarmonyLib;
using RiftOfTheNecroManager.Scripts;
using Shared.MenuOptions;
using Shared.Title;
using UnityEngine;

namespace RiftOfTheNecroManager.Patches;


public class SettingsMenuManagerState : State<SettingsMenuManager, SettingsMenuManagerState> {
    internal static RiftModsSettingsController? Controller { get; private set; }
    
    internal void CreateSettingsMenu() {
        if(Controller) {
            Log.Warning("Tried to create mod settings menu controller, but one already exists.");
            return;
        }
        
        // we load a bunch of objects from the accessibility menu and store them as prefabs
        // we clone these to create our own menus
        var template = Instance._riftAccessibilitySettingsController;
        var controller = RiftModsSettingsController.Create(new(template,
                Instance._accessibilityButton,
                template.GetComponentInChildren<ToggleOption>(),
                template.GetComponentInChildren<CarouselOptionGroup>(),
                template._backgroundDetailCarouselOptionPrefab,
                Instance._riftAudioSettingsController._sliderPrefab,
                template._cancelButton
        ));
        if(controller == null) {
            Log.Fatal("Failed to create mod settings menu controller.");
            return;
        }
        controller.AddAllModMenus();
        
        // add a button to the base settings menu
        var modsButton = Object.Instantiate(Instance._accessibilityButton, Instance._accessibilityButton.transform.parent);
        modsButton.name = "TextButton - Mods";
        modsButton.OnSubmit += () => {
            Instance._contentParent.SetActive(false);
            controller.gameObject.SetActive(true);
        };
        controller.OnClose += () => {
            if(!Instance.enabled) return;
            controller.ScheduleForNextFrame(() => {
                controller.gameObject.SetActive(false);
                Instance._contentParent.SetActive(true);
            });
        };
        
        foreach(var label in modsButton._textLabels) {
            Util.ForceSetText(label, "MODS");
        }
        
        // make it a different color than the other buttons
        var color = ColorText.Green.Color;
        modsButton._selectedTextColor = color;
        modsButton._unselectedTextColor = color.RGBMultiplied(0.5f);
        
        // add the button to the input controller and layout group as the penultimate option (before BACK)
        Instance._inputController.TryAddOption(modsButton, Instance._inputController.LastOptionIndex);
        var index = modsButton.transform.GetSiblingIndex();
        if(index > 0) {
            modsButton.transform.SetSiblingIndex(index - 1);
        }
        
        Controller = controller;
        Log.Info("Successfully created mod settings menu.");
    }
}

[HarmonyPatch(typeof(SettingsMenuManager))]
internal static class SettingsMenuManagerPatch {
    [HarmonyPatch(nameof(SettingsMenuManager.Start))]
    [HarmonyPostfix]
    public static void Start(SettingsMenuManager __instance) {
        var state = SettingsMenuManagerState.Of(__instance);
        state.CreateSettingsMenu();
    }
}
