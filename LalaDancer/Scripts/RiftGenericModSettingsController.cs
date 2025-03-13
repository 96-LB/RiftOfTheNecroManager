using BepInEx;
using BepInEx.Configuration;
using LalaDancer.Patches;
using Shared.MenuOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TicToc.Localization.Components;
using TMPro;
using UnityEngine;

namespace LalaDancer.Scripts;

using static UnityEngine.UIElements.StylePropertyAnimationSystem;
using Action = System.Action;

public class RiftGenericModSettingsController : MonoBehaviour {

    [SerializeField]
    private GameObject optionsObj;

    [SerializeField]
    private OptionsScreenInputController inputController;

    [SerializeField]
    private ScrollableSelectableOptionGroup optionsGroup;

    [SerializeField]
    private List<SelectableOption> buttons;
    
    public event Action OnClose;
    
    public bool Initialized { get; private set; }

    public PluginInfo PluginInfo { get; private set; }
    
    public GameObject OptionsObj => optionsObj;

    public ScrollableSelectableOptionGroup OptionsGroup => optionsGroup;
    
    public OptionsScreenInputController InputController => inputController;
    

    public static RiftGenericModSettingsController Create(RiftModsSettingsController other, PluginInfo plugin) {
        var copy = Instantiate(other, other.transform.parent);
        copy.gameObject.SetActive(false);
        copy.gameObject.name = $"ModSettingsScreen - {plugin.Metadata.Name}";

        var controller = copy.gameObject.AddComponent<RiftGenericModSettingsController>();
        controller.Initialized = true;
        controller.PluginInfo = plugin;
        controller.optionsObj = copy.OptionsObj;
        controller.optionsGroup = copy.OptionsGroup;
        controller.inputController = copy.InputController;
        controller.buttons = [];
        Destroy(copy);

        controller.OptionsGroup.RemoveAllOptions(true);
        
        var categories = plugin.Instance.Config.GroupBy(
            x => x.Key.Section,
            x => x
        );
        foreach(var category in categories) {
            controller.MakeHeader(category.Key);
            foreach(var option in category) {
                controller.MakeOption(option.Key, option.Value);
            }
        }

        var title = controller.OptionsObj.transform.Find("Menu_Settings_TitleText");
        if(title.TryGetComponent(out TMP_Text text)) {
            text.SetText(plugin.Metadata.Name);
        }
        if(title.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer); // the localizer will overwrite our text changes
        }

        return controller;
    }

    public void MakeHeader(string category) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.textButtonTemplate, true);
        button.name = $"Label - Mod - {PluginInfo.Metadata.Name} - {category}";

        foreach(var label in button.Field<TMP_Text[]>("_textLabels").Value) {
            // the localizer will try to change the text we set
            // remove it so this doesn't happen
            if(label.TryGetComponent(out BaseLocalizer localizer)) {
                Destroy(localizer);
            }
            label.SetText(category);
            label.fontStyle |= FontStyles.Italic;
        }

        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
    }

    public void MakeOption(ConfigDefinition key, ConfigEntryBase value) {
        SelectableOption button = value switch {
            ConfigEntry<bool> val => MakeToggleOption(key, val),
            ConfigEntry<Enum> val => MakeCarouselOption(key, val),
            _ => null
        };
        if(button) {
            buttons.Add(button);
        }
    }

    public ToggleOption MakeToggleOption(ConfigDefinition key, ConfigEntry<bool> value) {
        var button = (ToggleOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.toggleTemplate, true);
        button.isOn = value.Value;
        button.name = $"SwitchOption - Mod - {PluginInfo.Metadata.Name} - {key.Section}.{key.Key}";
        button.OnValueChanged += (isOn) => {
            value.Value = isOn;
            Debug.LogWarning($"Flipped {key.Key} to {isOn}");
        };

        // the localizer will try to change the text we set
        // remove it so this doesn't happen
        var label = button.Field<TMP_Text>("_labelText").Value;
        if(label.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer);
        }
        label.SetText(key.Key);

        return button;
    }

    public CarouselSubOption MakeCarouselOption(ConfigDefinition key, ConfigEntry<bool> value) {
        var button = (ToggleOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.carouselTemplate, true);
        button.isOn = value.Value;
        button.name = $"SwitchOption - Mod - {PluginInfo.Metadata.Name} - {key.Section}.{key.Key}";
        button.OnValueChanged += (isOn) => {
            value.Value = isOn;
            Debug.LogWarning($"Flipped {key.Key} to {isOn}");
        };

        // the localizer will try to change the text we set
        // remove it so this doesn't happen
        var label = button.Field<TMP_Text>("_labelText").Value;
        if(label.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer);
        }
        label.SetText(key.Key);

        return button;
    }

    private void Awake() {
        if(!Initialized) {
            throw new UnityException("RiftModsSettingsController should be created using static Create method.");
        }

        if(InputController) {
            InputController.OnCloseInput += HandleCloseInput;
        }
    }

    private void OnDestroy() {
        if(InputController) {
            InputController.OnCloseInput -= HandleCloseInput;
        }
    }

    private void OnEnable() {
        OptionsObj.SetActive(value: true);
        InputController.IsInputDisabled = false;
        InputController.SetSelectionIndex(0);
        OptionsGroup.SetSelectionIndex(0);
    }

    private void OnDisable() {
        OptionsObj.SetActive(value: false);
        inputController.IsInputDisabled = true;
    }

    private void HandleCloseInput() {
        OnClose?.Invoke();
        InputController.SetSelectionIndex(0);
    }
}
