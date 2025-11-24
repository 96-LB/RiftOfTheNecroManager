using System.Collections;
using System.Text;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;

namespace RiftOfTheNecroManager;


internal static class Util {
    public static string PascalToSpaced(string str) {
        if(string.IsNullOrEmpty(str)) {
            return str;
        }
        var result = new StringBuilder(str[0].ToString(), 2 * str.Length);
        for(int i = 1; i < str.Length; i++) {
            char c = str[i];
            if(char.IsUpper(c) && !char.IsUpper(str[i - 1])) {
                result.Append(' ');
            }
            result.Append(c);
        }
        return result.ToString();
    }

    public static void ForceSetText(TMP_Text label, string text) {
        if(!label) {
            return;
        }

        // the localizer will try to change the text we set
        // remove it so this doesn't happen
        if(label.TryGetComponent(out BaseLocalizer localizer)) {
            Object.Destroy(localizer);
        }
        label.SetText(text);
    }

    public static void ForceSetText(GameObject obj, string text) {
        ForceSetText(obj.GetComponentInChildren<TMP_Text>(), text);
    }

    public static void ScheduleForNextFrame(this MonoBehaviour obj, System.Action action) {
        IEnumerator coroutine() {
            yield return null;
            action();
        }
        obj.StartCoroutine(coroutine());
    }
}
