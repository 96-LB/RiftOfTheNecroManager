using HarmonyLib;
using LalaDancer.Scripts;
using Shared.MenuOptions;
using Shared.Title;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;

namespace LalaDancer.Patches;

using P = SettingsMenuManager;
using MenuState = State<SettingsMenuManager, MenuStateData>;
using Action = System.Action;

internal class MenuStateData {
    internal TextButtonOption modsButton;
    internal RiftModsSettingsController riftModsSettingsController;
    internal Action HandleOpenModsSettings;
}

[HarmonyPatch(typeof(P), "Start")]
internal static class SettingsMenuManagerPatch {
    internal static void Postfix(
        P __instance,
        OptionsScreenInputController ____inputController,
        GameObject ____contentParent,
        TextButtonOption ____otherButton
    ) {
        // we're using the OTHER button as a template to make our new button
        if(____otherButton) {
            var controller = new RiftModsSettingsController(); // TODO: obviously doesn't work
            var handler = HandleOpenModsSettings(____contentParent, controller);
            var modsButton = Object.Instantiate(____otherButton, ____otherButton.transform.parent);
            modsButton.name = "TextButton - Mods";
            modsButton.OnSubmit += handler;

            var label = modsButton.GetComponentInChildren<TMP_Text>();
            label.SetText("MODS");
            modsButton.Field<TMP_Text[]>("_textLabels").Set([label]);

            Color color = new(196f / 255, 241f / 255, 65f / 255);
            modsButton.Field<Color>("_selectedTextColor").Set(color);
            modsButton.Field<Color>("_unselectedTextColor").Set(color * 0.6f);

            // the localizer will try to change the text we set
            // remove it so this doesn't happen
            if(label.TryGetComponent(out BaseLocalizer localizer)) {
                Object.Destroy(localizer);
            }

            // add the button to the input controller and layout group as the penultimate option (before BACK)
            ____inputController.TryAddOption(modsButton, ____inputController.LastOptionIndex);
            var index = modsButton.transform.GetSiblingIndex();
            if(index > 0) {
                modsButton.transform.SetSiblingIndex(index - 1);
            }

            var state = MenuState.Of(__instance);
            state.modsButton = modsButton;
            state.riftModsSettingsController = controller;
            state.HandleOpenModsSettings = handler;            

            Plugin.Log.LogInfo("Created MODS menu button.");
        } else {
            Plugin.Log.LogError("Failed to create MODS menu button!");
        }
    }

    internal static Action HandleOpenModsSettings(
        GameObject contentParent,
        RiftModsSettingsController riftModsSettingsController
    ) {
        return () => {
            contentParent.SetActive(value: false);
            riftModsSettingsController.gameObject.SetActive(value: true);
        };
    }
}
