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
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.textButtonPrefab, true);
        button.name = $"Label - Mod - {PluginInfo.Metadata.Name} - {category}";

        foreach(var label in button._textLabels) {
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
            ConfigEntry<string> val => MakeCarouselOption(key, val),
            _ => null
        };
        if(!button && value.SettingType.IsEnum) {
            MakeCarouselOption(key, value, value.SettingType.GetEnumNames());
        }

        if(button) {
            buttons.Add(button);
        }
    }

    public ToggleOption MakeToggleOption(ConfigDefinition key, ConfigEntry<bool> value) {
        var button = (ToggleOption)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.togglePrefab, true);
        button.isOn = value.Value;
        button.name = $"ToggleOption - Mod - {PluginInfo.Metadata.Name} - {key.Section}.{key.Key}";
        button.OnValueChanged += (isOn) => {
            value.Value = isOn;
            Plugin.Log.LogInfo($"Updated config [{key.Section}.{key.Key}] to {isOn}.");
        };

        // the localizer will try to change the text we set
        // remove it so this doesn't happen
        var label = button._labelText;
        if(label.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer);
        }
        label.SetText(key.Key);

        return button;
    }
    
    public CarouselOptionGroup MakeCarouselOption(ConfigDefinition key, ConfigEntry<Enum> value) {
        if(value.Description.AcceptableValues is AcceptableValueList<string> vals) {
            return MakeCarouselOption(key, value, vals.AcceptableValues);
        } else {
            return null;
        }
    }

    public CarouselOptionGroup MakeCarouselOption(ConfigDefinition key, ConfigEntry<string> value) {
        if(value.Description.AcceptableValues is AcceptableValueList<string> vals) {
            return MakeCarouselOption(key, value, vals.AcceptableValues);
        } else {
            return null;
        }
    }

    public CarouselOptionGroup MakeCarouselOption(ConfigDefinition key, ConfigEntryBase value, string[] options) {
        var button = (CarouselOptionGroup)OptionsGroup.AddOptionFromPrefab(SettingsMenuManagerPatch_Internal.carouselPrefab, true);
        button.name = $"CarouselOption - Mod - {PluginInfo.Metadata.Name} - {key.Section}.{key.Key}";
        button.RemoveAllOptions(true);
        var selectedIndex = 0;
        foreach(var option in options) {
            if(value.Description.AcceptableValues?.IsValid(option) ?? true) {
                var subOption = Instantiate(SettingsMenuManagerPatch_Internal.carouselOptionPrefab, button.Content);
                subOption.name = $"CarouselSubOption - Mod - {PluginInfo.Metadata.Name} - {key.Section}.{key.Key} - {option}";
                subOption.SetPrimaryTextLabel(option);
                button.TryAddOption(subOption);
                if(string.Equals(option, value.GetSerializedValue(), StringComparison.InvariantCultureIgnoreCase)) {
                    selectedIndex = button.NumberOfOptions - 1;
                }
            }
        }

        // only generate the carousel if there are multiple options to choose from
        if(button.NumberOfOptions < 2) {
            button.RemoveAllOptions(true);
            Destroy(button.gameObject);
            return null;
        }

        button.SetSelectionIndex(selectedIndex);
        button.FlagAsExternallyInitialized();

        button.OnSelectedIndexChanged += (index) => {
            value.SetSerializedValue(options[index]);
            Plugin.Log.LogInfo($"Updated config [{key.Section}.{key.Key}] to {index} ({options[index]})");
        };

        // the localizer will try to change the text we set
        // remove it so this doesn't happen
        var label = button._title;
        if(label.TryGetComponent(out BaseLocalizer localizer)) {
            Destroy(localizer);
        }
        label.SetText(key.Key);

        return button;
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
    }

    protected void OnDisable() {
        OptionsObj.SetActive(value: false);
        inputController.IsInputDisabled = true;
    }

    protected void HandleCloseInput() {
        OnClose?.Invoke();
        InputController.SetSelectionIndex(0);
    }
}
