using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Shared;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace RiftOfTheNecroManager;


public static class Util {
    public static string GameVersion => BuildInfoHelper.Instance.BuildId.Split('-')[0];
    
    public static string PascalToSpaced(string str) {
        if(string.IsNullOrEmpty(str)) {
            return str;
        }
        var result = new StringBuilder(str[0].ToString(), 2 * str.Length);
        for(int i = 1; i < str.Length; i++) {
            char c = str[i];
            if(char.IsUpper(c) && !char.IsUpper(str[i - 1]) && !char.IsWhiteSpace(str[i - 1])) {
                result.Append(' ');
            }
            result.Append(c);
        }
        return result.ToString();
    }
    
    public static string RemoveWhitespace(string str) {
        if(string.IsNullOrEmpty(str)) {
            return str;
        }
        var result = new StringBuilder(str.Length);
        foreach(char c in str) {
            if(!char.IsWhiteSpace(c)) {
                result.Append(c);
            }
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
            UnityEngine.Object.Destroy(localizer);
        }
        label.SetText(text);
    }
    
    public static void ForceSetText(GameObject obj, string text) {
        ForceSetText(obj.GetComponentInChildren<TMP_Text>(), text);
    }
    
    public static void ScheduleForNextFrame(this MonoBehaviour obj, Action action) {
        IEnumerator coroutine() {
            yield return null;
            action();
        }
        obj.StartCoroutine(coroutine());
    }
    
    public static Assembly GetCallingAssembly(params Type[] typesToExclude) {
        var stackTrace = new StackTrace();
        for(int i = 1; i < stackTrace.FrameCount; i++) {
            var frame = stackTrace.GetFrame(i);
            var type = frame.GetMethod()?.DeclaringType;
            var assembly = type?.Assembly;
            if(assembly != null && !typesToExclude.Contains(type)) {
                return assembly;
            }
        }
        return Assembly.GetExecutingAssembly();
    }
    
    public static UnityWebRequestAsyncOperation SendJsonRequest(string url, object data, Action<UnityWebRequest> onCompleted) {
        var json = JsonConvert.SerializeObject(data);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        
        var request = new UnityWebRequest(url, "POST") {
            uploadHandler = new UploadHandlerRaw(jsonBytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        
        var operation = request.SendWebRequest();
        operation.completed += delegate { try {
            onCompleted(request);
        } finally {
            request.Dispose();
        } };
        
        return operation;
    }
}
