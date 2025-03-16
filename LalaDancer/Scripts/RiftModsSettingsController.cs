using BepInEx;
using BepInEx.Bootstrap;
using LalaDancer.Patches;
using Shared;
using Shared.MenuOptions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;

namespace LalaDancer.Scripts;

using Action = System.Action;
using ButtonHandler = (SelectableOption button, RiftGenericModSettingsController controller, System.Action handler);

public class RiftModsSettingsController : MonoBehaviour {
    [SerializeField]
    private GameObject optionsObj;

    [SerializeField]
    private OptionsScreenInputController inputController;

    [SerializeField]
    private ScrollableSelectableOptionGroup optionsGroup;

    [SerializeField]
    private List<ButtonHandler> buttons;
    
    public event Action OnClose;

    public bool Initialized { get; private set; }

    public GameObject OptionsObj => optionsObj;

    public ScrollableSelectableOptionGroup OptionsGroup => optionsGroup;

    public OptionsScreenInputController InputController => inputController;


    public static RiftModsSettingsController Create(RiftAccessibilitySettingsController other) {
        var copy = Instantiate(other, other.transform.parent);
        copy.gameObject.SetActive(false);
        copy.gameObject.name = "ModsSettingsScreen";

        var controller = copy.gameObject.AddComponent<RiftModsSettingsController>();
        controller.Initialized = true;
        controller.optionsObj = copy._mainOptionsParent;
        controller.optionsGroup = copy._scrollableSelectableOptionGroup;
        controller.inputController = copy._optionsScreenInputController;
        controller.buttons = [];
        DestroyImmediate(copy); // the calls to DestroyImmediate are needed here because we instantiate a copy later this frame

        foreach(var opt in controller.InputController._options) {
            if(opt.gameObject != controller.OptionsGroup.gameObject) {
                DestroyImmediate(opt.gameObject);
            }
        }
        foreach(var opt in controller.OptionsGroup._options) {
            DestroyImmediate(opt.gameObject);
        }
        controller.InputController.RemoveAllOptions();
        controller.OptionsGroup.RemoveAllOptions();
        controller.InputController.TryAddOption(controller.OptionsGroup);

        var plugins = Chainloader.PluginInfos.Values.OrderBy(x => x.Metadata.Name).ToArray();
        foreach(var plugin in plugins) {
            controller.MakeOption(plugin);
        }
        
        var title = controller.OptionsObj.transform.Find("Menu_Settings_TitleText");
        if(title.TryGetComponent(out TMP_Text text)) {
            text.SetText("MODS");
        }
        if(title.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer); // the localizer will overwrite our text changes
        }

        return controller;
    }

    public void MakeOption(PluginInfo plugin) {
        var controller = RiftGenericModSettingsController.Create(this, plugin);
        void HandleOpenModSettings() {
            Debug.LogError("Opening mod settings for " + plugin.Metadata.Name);
            OptionsObj.SetActive(false);
            InputController.IsInputDisabled = true;
            controller.gameObject.SetActive(true);
        }
        void HandleModSettingsClosed() {
            Debug.LogError("Closing menu!");
            StartCoroutine(CloseModSettingsRoutine(controller));
        }
        
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.textButtonPrefab, true);
        button.name = $"TextButton - Mod - {plugin.Metadata.Name}";
        button.OnSubmit += HandleOpenModSettings;

        foreach(var label in button._textLabels) {
            // the localizer will try to change the text we set
            // remove it so this doesn't happen
            if(label.TryGetComponent(out BaseLocalizer localizer)) {
                Destroy(localizer);
            }
            label.SetText(plugin.Metadata.Name);
        }

        buttons.Add((button, controller, HandleModSettingsClosed));
    }

    protected IEnumerator CloseModSettingsRoutine(RiftGenericModSettingsController controller) {
        yield return null;
        controller.gameObject.SetActive(false);
        InputController.IsInputDisabled = false;
        OptionsObj.SetActive(true);
    }

    protected void Awake() {
        if(!Initialized) {
            throw new UnityException("RiftModsSettingsController should be created using static Create method.");
        }

        if(InputController) {
            InputController.OnCloseInput += HandleCloseInput;
        }
    }

    protected void OnDestroy() {
        if(InputController) {
            InputController.OnCloseInput -= HandleCloseInput;
        }
    }

    protected void OnEnable() {
        OptionsObj.SetActive(value: true);
        InputController.IsInputDisabled = false;
        InputController.SetSelectionIndex(0);
        OptionsGroup.SetSelectionIndex(0);

        foreach(var (_, controller, handler) in buttons) {
            if(controller) {
                controller.OnClose += handler;
            }
        }
    }

    protected void OnDisable() {
        OptionsObj.SetActive(value: false);
        InputController.IsInputDisabled = true;

        foreach(var (_, controller, handler) in buttons) {
            if(controller) {
                controller.OnClose -= handler;
            }
        }
    }

    protected void HandleCloseInput() {
        OnClose?.Invoke();
        InputController.SetSelectionIndex(0);
    }
}
