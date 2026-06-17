using HarmonyLib;
using Shared.Feedback;
using TMPro;

namespace RiftOfTheNecroManager.Patches;


public class FeedbackControllerState : State<FeedbackController, FeedbackControllerState> {
    public TMP_InputField Comment => Instance._commentInputField;
    public TMP_Text? Placeholder => Comment.placeholder as TMP_Text;
    
    public void UpdatePlaceholder() {
        var text = "<b>WARNING: You have mods active!</b>";
        text += "\n\n";
        text += "If you are reporting a bug, please remove your mods, restart your game, and reproduce the issue before submitting a report.";
        text += " ";
        text += "If you are having a problem with a mod, please contact the mod developer instead.";
        Placeholder?.text = ColorText.Red.Text(text);
        Placeholder?.alpha = 1;
        Placeholder?.fontStyle &= ~FontStyles.Italic;
    }
}


[HarmonyPatch(typeof(FeedbackController))]
public static class FeedbackControllerPatch {
    [HarmonyPatch(nameof(FeedbackController.Show))]
    [HarmonyPostfix]
    public static void Show(FeedbackController __instance) {
        var state = FeedbackControllerState.Of(__instance);
        state.UpdatePlaceholder();
    }
}
