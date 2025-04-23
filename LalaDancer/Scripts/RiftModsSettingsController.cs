using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using FMODUnity;
using Shared;
using Shared.Audio;
using Shared.MenuOptions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LalaDancer.Scripts;


public class RiftModsSettingsController : MonoBehaviour {
    public static RiftAccessibilitySettingsController Template { get; private set; }
    public static TextButtonOption TextButtonPrefab { get; private set; }
    public static ToggleOption TogglePrefab { get; private set; }
    public static CarouselOptionGroup CarouselPrefab { get; private set; }
    public static CarouselSubOption CarouselOptionPrefab { get; private set; }
    public static MenuButtonOption BackButtonPrefab { get; private set; }
    public static bool AllPrefabsLoaded => TextButtonPrefab && TogglePrefab && CarouselPrefab && CarouselOptionPrefab && BackButtonPrefab;

    public bool Initialized { get; private set; }
    public GameObject OptionsObj { get; private set; }
    public ScrollableSelectableOptionGroup OptionsGroup { get; private set; }
    public OptionsScreenInputController InputController { get; private set; }
    public MenuButtonOption BackButton { get; private set; }
    public EventReference CancelSelectionSfx { get; private set; }

    public event Action OnClose;

    public static void LoadPrefabs(
        RiftAccessibilitySettingsController template,
        TextButtonOption accessibilityButton,
        ToggleOption togglePrefab,
        CarouselOptionGroup carouselPrefab,
        CarouselSubOption carouselOptionPrefab,
        MenuButtonOption backButtonPrefab
    ) {
        if(!template) {
            throw new UnityException("Failed to load prefab for mod menu. This is usually loaded by cloning the accessibility settings menu.");
        }
        if(!accessibilityButton) {
            throw new UnityException("Failed to load prefab for text buttons. This is usually loaded by cloning the button for the accessibility settings menu.");
        }
        if(!togglePrefab) {
            throw new UnityException("Failed to load prefab for toggle options. This is usually loaded by cloning a toggle option from the accessibility settings menu.");
        }
        if(!carouselPrefab) {
            throw new UnityException("Failed to load prefab for carousel options. This is usually loaded by cloning a carousel option from the accessibility settings menu.");
        }
        if(!carouselOptionPrefab) {
            throw new UnityException("Failed to load prefab for carousel suboptions. This is usually loaded by cloning a carousel suboption from the accessibility settings menu.");
        }
        if(!backButtonPrefab) {
            throw new UnityException("Failed to load prefab for back button. This is usually loaded by cloning the back button from the accessibility settings menu.");
        }

        Template = template;
        TextButtonPrefab = accessibilityButton;
        TogglePrefab = togglePrefab;
        CarouselPrefab = carouselPrefab;
        CarouselOptionPrefab = carouselOptionPrefab;
        BackButtonPrefab = backButtonPrefab;
    }

    public static RiftModsSettingsController Create(string title = "MODS", string name = "ModsSettingsScreen") {
        if(!AllPrefabsLoaded) {
            throw new UnityException($"{nameof(RiftModsSettingsController)} could not be created because not all prefabs are loaded. This usually means that your mod version is outdated. If you are using the latest version, please contact the mod developers.");
        }

        var copy = Instantiate(Template, Template.transform.parent);
        copy.gameObject.SetActive(false);
        copy.gameObject.name = name;

        var controller = copy.gameObject.AddComponent<RiftModsSettingsController>();
        controller.OptionsObj = copy._mainOptionsParent;
        controller.OptionsGroup = copy._scrollableSelectableOptionGroup;
        controller.InputController = copy._optionsScreenInputController;
        controller.CancelSelectionSfx = copy._cancelSelectionSfx;

        Destroy(copy);
        Destroy(controller.transform.Find("ColorBlindnessSubmenu").gameObject);

        // replace the back/confirm button group with just the back button
        var backMenu = (SelectableOptionGroup)controller.InputController._options[1];
        var backButton = (MenuButtonOption)backMenu._options[0];
        var backButtonTransform = backButton.GetComponent<RectTransform>();
        backButtonTransform.SetParent(controller.OptionsObj.transform, false);
        backButtonTransform.pivot = new(0.5f, 0.5f);
        backButtonTransform.anchorMin = backButtonTransform.anchorMax = new(0.5f, 0f);
        backButtonTransform.anchoredPosition = new(0, 200);
        Destroy(backMenu.gameObject);

        backButton.OnClick += controller.HandleCloseInput;
        backButton.OnClick += controller.PlayCancelSfx;
        controller.BackButton = backButton;

        // all old options are removed here because of a hidden call to Initialize()
        controller.InputController.TryAddOption(controller.OptionsGroup);
        controller.InputController.TryAddOption(backButton);

        foreach(var opt in controller.OptionsGroup._options) {
            DestroyImmediate(opt.gameObject);
        }
        controller.OptionsGroup.RemoveAllOptions();

        var titleObj = controller.OptionsObj.transform.Find("Menu_Settings_TitleText");
        Util.ForceSetText(titleObj.gameObject, title);

        controller.Initialized = true;
        return controller;
    }

    public void AddAllModMenus() {
        var plugins = Chainloader.PluginInfos.Values.OrderBy(x => x.Metadata.Name);
        foreach(var plugin in plugins) {
            AddModMenu(plugin);
        }
    }

    public void AddModMenu(PluginInfo plugin) {
        var controller = Create(Util.PascalToSpaced(plugin.Metadata.Name), $"ModSettingsScreen - {plugin.Metadata.Name}");
        controller.AddAllConfigOptions(plugin);

        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(TextButtonPrefab, true);
        button.name = $"TextButton - Mod - {plugin.Metadata.Name}";

        button.OnSubmit += () => {
            OptionsObj.SetActive(false);
            InputController.IsInputDisabled = true;
            controller.gameObject.SetActive(true);
        };

        controller.OnClose += () => {
            if(!enabled) {
                return;
            }
            button.SetSubmitted(false);
            this.ScheduleForNextFrame(() => {
                controller.gameObject.SetActive(false);
                InputController.IsInputDisabled = false;
                OptionsObj.SetActive(true);
            });
        };

        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, Util.PascalToSpaced(plugin.Metadata.Name));
        }

        controller.gameObject.SetActive(true);
        controller.gameObject.SetActive(false);
    }

    public void AddAllConfigOptions(PluginInfo plugin) {
        var categories = plugin.Instance.Config.Select(x => x.Key.Section).Distinct();
        foreach(var category in categories) {
            AddConfigCategory(plugin, category);
        }
    }

    public void AddConfigCategory(PluginInfo plugin, string category) {
        AddCategoryHeader(plugin, category);

        var options = plugin.Instance.Config.Where(x => x.Key.Section == category).OrderBy(x => x.Key.Key);
        foreach(var option in options) {
            AddConfigOption(plugin, option.Key, option.Value);
        }
    }

    public void AddCategoryHeader(PluginInfo plugin, string category) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(TextButtonPrefab, true);
        button.name = $"Label - Mod - {plugin.Metadata.Name} - {category}";

        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, category);
            label.fontStyle |= FontStyles.Italic;
        }

        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
    }

    public SelectableOption AddConfigOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value) {
        return value switch {
            ConfigEntry<bool> val => AddToggleOption(plugin, key, val),
            ConfigEntry<string> val => AddCarouselOption(plugin, key, val),
            _ when value.SettingType.IsEnum => AddCarouselOption(plugin, key, value, value.SettingType.GetEnumNames()),
            _ => null
        };
    }

    public ToggleOption AddToggleOption(PluginInfo plugin, ConfigDefinition key, ConfigEntry<bool> value) {
        var button = (ToggleOption)OptionsGroup.AddOptionFromPrefab(TogglePrefab, true);
        button.isOn = value.Value;
        button.name = $"ToggleOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        button.OnValueChanged += (isOn) => {
            value.Value = isOn;
            Plugin.Log.LogInfo($"Updated config [{key.Section}.{key.Key}] to {isOn}.");
        };
        Util.ForceSetText(button._labelText, key.Key);
        return button;
    }

    public CarouselOptionGroup AddCarouselOption(PluginInfo plugin, ConfigDefinition key, ConfigEntry<string> value) {
        if(value.Description.AcceptableValues is AcceptableValueList<string> vals) {
            return AddCarouselOption(plugin, key, value, vals.AcceptableValues);
        } else {
            return null;
        }
    }

    public CarouselOptionGroup AddCarouselOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value, string[] options) {
        var carousel = (CarouselOptionGroup)OptionsGroup.AddOptionFromPrefab(CarouselPrefab, true);
        carousel.name = $"CarouselOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        carousel.RemoveAllOptions(true);
        var selectedIndex = 0;
        var width = 300f; // minimum width
        foreach(var option in options) {
            if(value.Description.AcceptableValues?.IsValid(option) ?? true) {
                var subOption = Instantiate(CarouselOptionPrefab, carousel.Content);
                subOption.name = $"CarouselSubOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key} - {option}";
                
                // set text and measure width
                if(subOption._textLabels != null && subOption._textLabels.Length > 0) {
                    var text = Util.PascalToSpaced(option);
                    var label = subOption._textLabels[0];
                    width = Mathf.Max(width, label.GetPreferredValues(text).x + 10f);
                    Util.ForceSetText(label, text);
                }

                carousel.TryAddOption(subOption);
                if(string.Equals(option, value.GetSerializedValue(), StringComparison.InvariantCultureIgnoreCase)) {
                    selectedIndex = carousel.NumberOfOptions - 1;
                }
            }
        }

        // only generate the carousel if there are multiple options to choose from
        if(carousel.NumberOfOptions < 2) {
            carousel.RemoveAllOptions(true);
            Destroy(carousel.gameObject);
            return null;
        }

        // set the title text
        Util.ForceSetText(carousel._title, key.Key);

        // set the width of the buttons
        var rect = carousel.Content.parent.GetComponent<RectTransform>();
        rect.sizeDelta = new(width, rect.sizeDelta.y);
        foreach(var arrow in carousel._arrows) {
            var rect2 = arrow.GetComponent<RectTransform>();
            rect2.anchoredPosition = new((30f + width / 2f) * Mathf.Sign(rect2.anchoredPosition.x), rect2.anchoredPosition.y);
        }
        
        // initialize the carousel
        carousel.SetSelectionIndex(selectedIndex);
        carousel.FlagAsExternallyInitialized();

        // make the carousel set the config value
        carousel.OnSelectedIndexChanged += (index) => {
            value.SetSerializedValue(options[index]);
            Plugin.Log.LogInfo($"Updated config [{key.Section}.{key.Key}] to {index} ({options[index]})");
        };

        return carousel;
    }

    protected void Awake() {
        if(!Initialized) {
            throw new UnityException($"{nameof(RiftModsSettingsController)} should be created using static {nameof(Create)} method.");
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
        OptionsObj.SetActive(true);
        InputController.IsInputDisabled = false;
        InputController.SetSelectionIndex(0);
        OptionsGroup.SetSelectionIndex(0);
    }

    protected void OnDisable() {
        OptionsObj.SetActive(false);
        InputController.IsInputDisabled = true;
    }

    protected void HandleCloseInput() {
        OnClose?.Invoke();
        InputController.SetSelectionIndex(0);
    }

    protected void PlayCancelSfx() {
        if(!CancelSelectionSfx.IsNull) {
            AudioManager.Instance.PlayAudioEvent(CancelSelectionSfx, 0f, shouldCache: true, 0u, 0f, shouldApplyLatency: false);
        }
    }
}
