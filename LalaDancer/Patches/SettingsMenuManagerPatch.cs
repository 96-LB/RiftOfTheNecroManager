using BepInEx.Configuration;
using HarmonyLib;
using LalaDancer.Scripts;
using Shared;
using Shared.MenuOptions;
using Shared.Title;
using System.Collections;
using System.Xml.Linq;
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
    internal Action HandleModsSettingsClosed;
}

[HarmonyPatch(typeof(P))]
internal static class SettingsMenuManagerPatch {
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    internal static void Start(
        P __instance,
        OptionsScreenInputController ____inputController,
        GameObject ____contentParent,
        TextButtonOption ____backButton,
        RiftAccessibilitySettingsController ____riftAccessibilitySettingsController
    ) {
        // the back button is our template for text buttons
        SettingsMenuManagerPatch_Internal.textButtonTemplate = ____backButton;
        if(!____backButton) {
            Plugin.Log.LogError("Failed to find back button on settings menu. Aborting mod settings menu creation.");
            return;
        }

        // the accessibility menu is our template for the new menus
        if(!____riftAccessibilitySettingsController) {
            Plugin.Log.LogError("Failed to find accessibility settings menu. Aborting mod settings menu creation.");
            return;
        }

        var controller = RiftModsSettingsController.Create(____riftAccessibilitySettingsController);
        var HandleOpenModsSettings = SettingsMenuManagerPatch_Internal.HandleOpenModsSettings(____contentParent, controller);
        var HandleModsSettingsClosed = SettingsMenuManagerPatch_Internal.HandleModsSettingsClosed(____contentParent, controller);
        var modsButton = Object.Instantiate(____backButton, ____backButton.transform.parent);
        modsButton.name = "TextButton - Mods";
        modsButton.OnSubmit += HandleOpenModsSettings;

        foreach(var label in modsButton.Field<TMP_Text[]>("_textLabels").Value) {
            // the localizer will try to change the text we set
            // remove it so this doesn't happen
            if(label.TryGetComponent(out BaseLocalizer localizer)) {
                Object.Destroy(localizer);
            }
            label.SetText("MODS");
        }

        Color color = new(196f / 255, 241f / 255, 65f / 255);
        modsButton.Field<Color>("_selectedTextColor").Set(color);
        modsButton.Field<Color>("_unselectedTextColor").Set(color * 0.6f);

        // add the button to the input controller and layout group as the penultimate option (before BACK)
        ____inputController.TryAddOption(modsButton, ____inputController.LastOptionIndex);
        var index = modsButton.transform.GetSiblingIndex();
        if(index > 0) {
            modsButton.transform.SetSiblingIndex(index - 1);
        }

        var state = MenuState.Of(__instance);
        state.modsButton = modsButton;
        state.riftModsSettingsController = controller;
        state.HandleOpenModsSettings = HandleOpenModsSettings;
        state.HandleModsSettingsClosed = HandleModsSettingsClosed;

        Plugin.Log.LogInfo("Created mods menu button.");
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnEnable")]
    internal static void OnEnable(P __instance) {
        var state = MenuState.Of(__instance);
        if(state.riftModsSettingsController) {
            state.riftModsSettingsController.OnClose += state.HandleModsSettingsClosed;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnDisable")]
    internal static void OnDisable(P __instance) {
        var state = MenuState.Of(__instance);
        if(state.riftModsSettingsController) {
            state.riftModsSettingsController.OnClose -= state.HandleModsSettingsClosed;
        }
    }
}

internal static class SettingsMenuManagerPatch_Internal {

    internal static TextButtonOption textButtonTemplate;
    

    internal static Action HandleOpenModsSettings(
        GameObject contentParent,
        RiftModsSettingsController riftModsSettingsController
    ) {
        return () => {
            contentParent.SetActive(false);
            riftModsSettingsController.gameObject.SetActive(true);
        };
    }

    internal static Action HandleModsSettingsClosed(
        GameObject contentParent,
        RiftModsSettingsController riftModsSettingsController
    ) {
        return () => {
            riftModsSettingsController.StartCoroutine(CloseModsSettingsRoutine(contentParent, riftModsSettingsController));
        };
    }

    internal static IEnumerator CloseModsSettingsRoutine(
        GameObject contentParent,
        RiftModsSettingsController riftModsSettingsController
    ) {
        yield return null;
        riftModsSettingsController.gameObject.SetActive(false);
        contentParent.SetActive(true);
    }
}
