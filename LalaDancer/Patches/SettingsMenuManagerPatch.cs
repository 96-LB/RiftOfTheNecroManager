using HarmonyLib;
using Shared.MenuOptions;
using Shared.Title;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LalaDancer.Patches;

using P = SettingsMenuManager;
using MenuState = State<SettingsMenuManager, MenuStateData>;

internal class MenuStateData {
    internal TextButtonOption modsButton;
}

[HarmonyPatch(typeof(P), "Start")]
internal static class SettingsMenuManagerPatch {
    internal static void Postfix(
        P __instance,
        GameObject ____contentParent,
        OptionsScreenInputController ____inputController,
        TextButtonOption ____otherButton
    ) {
        Plugin.Log.LogWarning("SettingsMenuManager started");
        if(____otherButton) {
            var modsButton = Object.Instantiate(____otherButton, ____contentParent.transform);
            var state = MenuState.Of(__instance);
            state.modsButton = modsButton;
            Plugin.Log.LogWarning($"Made mods button: {modsButton}");

            var text = modsButton.Field<TMP_Text[]>("_textLabels");
            foreach(var t in text.Value) {
                Plugin.Log.LogWarning(t);
                t.text = "MODS";
                Plugin.Log.LogWarning(t.text);
                t.ForceMeshUpdate();
            }
            modsButton.Field<Color>("_selectedTextColor").Set(Color.red);

            ____inputController.TryAddOption(modsButton, ____inputController.LastOptionIndex);
        } else {
            Plugin.Log.LogError("Failed to create mod button!");
        }
    }
}
