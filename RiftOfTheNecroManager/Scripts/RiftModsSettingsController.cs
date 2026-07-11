using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using FMOD.Studio;
using FMODUnity;
using Shared;
using Shared.Audio;
using Shared.MenuOptions;
using Shared.RiftInput;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiftOfTheNecroManager.Scripts;


public class RiftModsSettingsController : MonoBehaviour {
    public record ModMenu(SelectableOption Button, RiftModsSettingsController Menu);
    
    public record SettingsMenuTemplate(
        RiftAccessibilitySettingsController Template,
        TextButtonOption TextButton,
        ToggleOption ToggleOption,
        CarouselOptionGroup CarouselOptionGroup,
        CarouselSubOption CarouselSubOption,
        SliderOption SliderOption,
        MenuButtonOption BackButton
    );
    
    public SettingsMenuTemplate Template { get; private set; } = null!; // set in Create, throws an error if instantiated otherwise
    
    public bool Initialized { get; private set; }
    public GameObject OptionsObj { get; private set; } = null!;
    public ScrollableSelectableOptionGroup OptionsGroup { get; private set; } = null!;
    public OptionsScreenInputController InputController { get; private set; } = null!;
    public MenuButtonOption BackButton { get; private set; } = null!;
    public TextButtonOption DescriptionLabel { get; private set; } = null!;
    public Dictionary<SelectableOption, string> Descriptions { get; } = [];
    public Dictionary<PluginInfo, ModMenu> ModMenus { get; } = [];
    
    public event Action? OnClose;
    
    public static RiftModsSettingsController? Create(SettingsMenuTemplate template, string title = "MODS", string name = "ModsSettingsScreen") {
        
        var copy = Instantiate(template.Template, template.Template.transform.parent);
        copy.gameObject.SetActive(false);
        copy.gameObject.name = name;
        
        var controller = copy.gameObject.AddComponent<RiftModsSettingsController>();
        controller.Template = template;
        controller.OptionsObj = copy._mainOptionsParent;
        controller.OptionsGroup = copy._scrollableSelectableOptionGroup;
        controller.InputController = copy._optionsScreenInputController;
        
        Destroy(copy);
        Destroy(controller.transform.Find("ColorBlindnessSubmenu").gameObject);
        
        // replace the back/confirm button group with just the back button
        var backMenu = (SelectableOptionGroup)controller.InputController._options[1];
        var backButton = (MenuButtonOption)backMenu._options[0];
        var backTransform = backButton.GetComponent<RectTransform>();
        backTransform.SetParent(controller.OptionsObj.transform, false);
        backTransform.pivot = new(1, 0);
        backTransform.anchorMin = backTransform.anchorMax = new(0.5f, 0f);
        backTransform.anchoredPosition = new(-20, 200);
        backTransform.sizeDelta = new(200, 60);
        Destroy(backMenu.gameObject);
        
        backButton.OnClick += controller.HandleCloseInput;
        backButton.OnClick += controller.PlayCancelSfx;
        controller.BackButton = backButton;
        
        var descriptionLabel = Instantiate(template.TextButton, controller.OptionsObj.transform);
        var descriptionTransform = descriptionLabel.GetComponent<RectTransform>();
        descriptionLabel.name = $"Label - Mod - {name} - Description";
        descriptionTransform.pivot = new(0, 0);
        descriptionTransform.anchorMin = backTransform.anchorMax = new(0.5f, 0f);
        descriptionTransform.anchoredPosition = new(20, 160);
        descriptionTransform.sizeDelta = new(666, 100);
        foreach(var label in descriptionLabel._textLabels) {
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.fontSize *= 0.55f;
            label.fontSizeMin *= 0.55f;
            label.fontSizeMax *= 0.55f;
            label.enableAutoSizing = false;
            if(label.TryGetComponent<ContentSizeFitter>(out var fitter)) {
                Destroy(fitter);
            }
            var rect = label.GetComponent<RectTransform>();
            rect.pivot = rect.anchorMin = rect.anchorMax = rect.anchoredPosition = new();
            rect.sizeDelta = descriptionTransform.sizeDelta;
        }
        descriptionLabel.SetSelected(false, false);
        controller.DescriptionLabel = descriptionLabel;
        
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
    
    
    public void SetRectHeight(SelectableOption opt, float height) {
        if(opt.TryGetComponent<RectTransform>(out var rect)) {
            if(opt.TryGetComponent<ContentSizeFitter>(out var fitter)) {
                Destroy(fitter);
            }
            float delta = height - rect.rect.height;
            OptionsGroup._accumulatedContentSize += delta;
            rect.sizeDelta += Vector2.up * delta;
        }
    }
    
    public void AddAllModMenus() {
        var plugins = Chainloader.PluginInfos.Values.OrderBy(x => x.Metadata.Name);
        foreach(var plugin in plugins) {
            AddModMenu(plugin);
        }
    }
    
    public void AddModMenu(PluginInfo plugin) {
        if(ModMenus.ContainsKey(plugin)) {
            Log.Warning($"Tried to add mod menu for plugin {plugin.Metadata.GUID}, but one already exists!");
            return;
        }

        var info = RiftPluginInfo.Of(plugin);
        var title = info.GetMenuName();
        var controller = Create(Template, title, $"ModSettingsScreen - {plugin.Metadata.Name}");
        if(controller == null) {
            Log.Fatal($"Failed to create settings controller for mod {plugin.Metadata.Name}.");
            return;
        }
        
        controller.AddAllConfigOptions(plugin);
        
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"TextButton - Mod - {plugin.Metadata.Name}";
        
        button.OnSubmit += () => {
            OptionsObj?.SetActive(false);
            InputController?.Pipe(x => x.IsInputDisabled = true);
            controller.gameObject.SetActive(true);
        };
        
        controller.OnClose += () => {
            if(!enabled) {
                return;
            }
            button.SetSubmitted(false);
            this.ScheduleForNextFrame(() => {
                controller.gameObject.SetActive(false);
                InputController?.Pipe(x => x.IsInputDisabled = false);
                OptionsObj?.SetActive(true);
            });
        };
        
        foreach(var label in button._textLabels) {
            var text = title;
            if(info.IsNecroManagerPlugin) {
                text += "<voffset=0.2em><size=75%>";
                text += info.Deactivated ? $"<sprite=23 color={ColorText.Red}>" // x
                    : info.Incompatible ? $"<sprite=26 color={ColorText.Orange}>" // orange triangle
                    : $"<sprite=25 color={ColorText.Green}>"; // green square
                text += "</size></voffset>";
                if(info.UpdateAvailable) {
                    text += $"<sprite=6 color={ColorText.Blue}>"; // blue sparkle
                }
            }
            Util.ForceSetText(label, text);
        }
        
        SetRectHeight(button, 60);
        
        var description = $"{info.GUID}\nv{info.Version}";
        if(info.UpdateAvailable) {
            description += ColorText.Blue.Text(" (Update available!)");
        }
        Descriptions[button] = description;
        
        ModMenus[plugin] = new(button, controller);
    }
    
    public void AddAllConfigOptions(PluginInfo plugin) {
        var categories = plugin.Instance.Config.Select(x => x.Key.Section).Distinct();
        foreach(var category in categories) {
            AddConfigCategory(plugin, category);
        }
    }
    
    public void AddConfigCategory(PluginInfo plugin, string category) {
        AddCategoryLabel(plugin, category);
        AddPadding(plugin);
        var options = plugin.Instance.Config.Where(x => x.Key.Section == category);
        foreach(var option in options) {
            AddConfigOption(plugin, option.Key, option.Value);
        }
    }
    
    public TextButtonOption? AddCategoryLabel(PluginInfo plugin, string category) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"Label - Mod - {plugin.Metadata.Name} - {category}";
        
        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, category);
            label.fontStyle |= FontStyles.Italic;
        }
        
        SetRectHeight(button, 75);
        
        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
        return button;
    }
    
    public SelectableOption? AddConfigOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value) =>
        value switch {
            ConfigEntry<bool> val => AddToggleOption(plugin, key, val),
            ConfigEntry<string> val => AddStringOrCarouselOption(plugin, key, val),
            _ when value.SettingType.IsEnum => AddCarouselOption(plugin, key, value, value.SettingType.GetEnumNames()),
            ConfigEntry<int> or ConfigEntry<float> => AddSliderOption(plugin, key, value),
            ConfigEntry<Color> val => AddColorOption(plugin, key, val),
            _ => null
        };
    
    public ToggleOption? AddToggleOption(PluginInfo plugin, ConfigDefinition key, ConfigEntry<bool> value) {
        var button = (ToggleOption)OptionsGroup.AddOptionFromPrefab(Template.ToggleOption, true);
        button.isOn = value.Value;
        button.name = $"ToggleOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        button.OnValueChanged += (isOn) => {
            value.Value = isOn;
            Log.Info($"Updated config [{key.Section}.{key.Key}] to {isOn}.");
        };
        Util.ForceSetText(button._labelText, key.Key);
        Descriptions[button] = value.Description.Description;
        return button;
    }
    
    public SelectableOption? AddStringOrCarouselOption(PluginInfo plugin, ConfigDefinition key, ConfigEntry<string> value) =>
        value.Description.AcceptableValues switch {
            AcceptableValueList<string> vals => AddCarouselOption(plugin, key, value, vals.AcceptableValues),
            _ => AddStringOption(plugin, key, value)
        };
    
    public CarouselOptionGroup? AddCarouselOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value, string[] options) {
        var carousel = (CarouselOptionGroup)OptionsGroup.AddOptionFromPrefab(Template.CarouselOptionGroup, true);
        carousel.name = $"CarouselOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        carousel.RemoveAllOptions(true);
        var selectedIndex = 0;
        var width = 300f; // minimum width
        foreach(var option in options) {
            if(value.Description.AcceptableValues?.IsValid(option) ?? true) {
                var subOption = Instantiate(Template.CarouselSubOption, carousel.Content);
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
        
        // reduce the enormous amount of space
        SetRectHeight(carousel, 100);
        
        // initialize the carousel
        carousel.SetSelectionIndex(selectedIndex);
        carousel.FlagAsExternallyInitialized();
        carousel.SetSelected(false, false);
        
        // make the carousel set the config value
        carousel.OnSelectedIndexChanged += (index) => {
            value.SetSerializedValue(options[index]);
            Log.Info($"Updated config [{key.Section}.{key.Key}] to {index} ({options[index]})");
        };
        
        Descriptions[carousel] = value.Description.Description;
        
        return carousel;
    }
    
    public SliderOption? AddSliderOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value) =>
        value.Description.AcceptableValues switch {
            AcceptableValueRange<int> val => AddSliderOption(plugin, key, value, val),
            AcceptableValueRange<float> val => AddSliderOption(plugin, key, value, val),
            _ => null
        };
    
    public SliderOption? AddSliderOption(
        PluginInfo plugin,
        ConfigDefinition key,
        ConfigEntryBase? value,
        AcceptableValueRange<float> range,
        Action<string, float>? onValueChanged = null
    ) {
        var slider = (SliderOption)OptionsGroup.AddOptionFromPrefab(Template.SliderOption, true);
        slider.name = $"SliderOption - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        
        slider._displayAsPercentage = false;
        slider._decimals = 3;
        slider._valueMin = range.MinValue;
        slider._valueMax = range.MaxValue;
        slider._valueStep = (range.MaxValue - range.MinValue) / 500f;
        slider._initialUpdateCooldown = 0.03f;
        slider._updateCooldownReductionInterval = 0.01f;
        slider._updateCooldownReductionAmount = 0.00025f;
        
        slider._value = Convert.ToSingle(value?.BoxedValue);
        Util.ForceSetText(slider._labelText, key.Key);
        slider.HandleValueUpdated();
        
        slider.OnValueChanged += onValueChanged ?? ((_, num) => {
            num = Mathf.Clamp(Mathf.Round(num * 1e6f) / 1e6f, range.MinValue, range.MaxValue);
            value?.SetSerializedValue(num.ToString(CultureInfo.InvariantCulture));
            Log.Info($"Updated config [{key.Section}.{key.Key}] to {num}.");
        });
        
        SetRectHeight(slider, 75);
        
        Descriptions[slider] = value?.Description.Description ?? "";
        
        return slider;
    }
    
    public SliderOption? AddSliderOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value, AcceptableValueRange<int> range) {
        var slider = AddSliderOption(plugin, key, value, new AcceptableValueRange<float>(range.MinValue, range.MaxValue));
        slider?.Pipe(x => {
            x._decimals = 0;
            x._valueStep = 1;
            x._initialUpdateCooldown = 0.15f;
            x._updateCooldownReductionAmount = 0.00125f;
        });
        return slider;
    }
    
    public TextButtonOption? AddColorLabel(PluginInfo plugin, ConfigDefinition key) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"Label - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        
        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, key.Key);
            label.fontSize *= 0.75f;
            label.fontSizeMin *= 0.75f;
            label.fontSizeMax *= 0.75f;
        }
        
        SetRectHeight(button, 40);
        
        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
        return button;
    }
    
    public TextButtonOption? AddPadding(PluginInfo plugin, float height = 10) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"Padding - Mod - {plugin.Metadata.Name}";
        
        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, "");
        }
        
        SetRectHeight(button, height);
        
        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
        return button;
    }
    
    public TextButtonOption? AddColorOption(PluginInfo plugin, ConfigDefinition key, ConfigEntry<Color> value) {
        var header = AddColorLabel(plugin, key);
        
        var range = new AcceptableValueRange<float>(0f, 1f);
        var sliders = new SliderOption[4];
        var channels = new[] { "Red", "Green", "Blue", "Alpha" };
        
        void OnValueChanged(string channel, float num) {
            var color = new Color(
                Mathf.Clamp01(sliders[0]._value),
                Mathf.Clamp01(sliders[1]._value),
                Mathf.Clamp01(sliders[2]._value),
                Mathf.Clamp01(sliders[3]._value)
            );
            
            var opaque = new Color(color.r, color.g, color.b);
            
            value.Value = color;
            for(int i = 0; i < sliders.Length; i++) {
                var bgColor = i == 3 ? color : opaque;
                sliders[i]._leftTrackSelectedColor = bgColor;
                sliders[i]._rightTrackSelectedColor = bgColor.RGBMultiplied(0.5f);
                if(sliders[i].IsSelected && sliders[i]._leftTrackBackground) {
                    sliders[i]._leftTrackBackground.color = bgColor;
                    sliders[i]._rightTrackBackground.color = bgColor.RGBMultiplied(0.5f);
                }
            }
            if(!string.IsNullOrEmpty(channel)) {
                Log.Info($"Updated config [{key.Section}.{key.Key}] {channel} to {num}.");
            }
        }
        
        for(int i = 0; i < sliders.Length; i++) {
            var slider = AddSliderOption(plugin, key, null, range, OnValueChanged);
            if(slider == null) {
                continue;
            }
            
            slider.name += " - " + channels[i];
            slider._value = value.Value[i];
            slider._valueId = channels[i];
            slider.HandleValueUpdated();
            Util.ForceSetText(slider._labelText, channels[i]);
            SetRectHeight(slider, 30);
            Descriptions[slider] = value.Description.Description;
            
            sliders[i] = slider;
        }
        
        OnValueChanged("", 0);
        
        var footer = AddPadding(plugin);
        
        return header; // this is cursed
    }
    
    public TextButtonOption? AddStringLabel(PluginInfo plugin, ConfigDefinition key) {
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"Label - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        
        foreach(var label in button._textLabels) {
            Util.ForceSetText(label, key.Key);
        }
        
        SetRectHeight(button, 40);
        
        OptionsGroup.RemoveOption(button);
        Destroy(button); // keeps the GameObject, but not the SelectableOption
        return button;
    }
    
    public TextButtonOption? AddStringOption(PluginInfo plugin, ConfigDefinition key, ConfigEntryBase value) {
        var header = AddStringLabel(plugin, key);
        
        var button = (TextButtonOption)OptionsGroup.AddOptionFromPrefab(Template.TextButton, true);
        button.name = $"String - Mod - {plugin.Metadata.Name} - {key.Section}.{key.Key}";
        button._submitEventRef = Sfx.Confirm;
        
        void SetText(string str, bool blinker = false) {
            static string Blinker(bool on) => $"<color=#{(on ? "d6f141" : "000000")}>|</color>";
            var escaped = str.Replace("</noparse>", "<<i></i>/noparse>");
            var text = $"{Blinker(false)}<noparse>{escaped}</noparse>{Blinker(blinker)}";
            foreach(var label in button._textLabels) {
                Util.ForceSetText(label, text);
            }
        }
        
        SetRectHeight(button, 35);
        SetText(value.GetSerializedValue());
        foreach(var label in button._textLabels) {
            label.fontSize *= 0.5f;
            label.fontSizeMin *= 0.5f;
            label.fontSizeMax *= 0.5f;
            label.fontStyle = FontStyles.Normal;
        }
        
        button.OnSubmit += async () => {
            InputController?.IsInputDisabled = true;
            button._selectedIndicator.SetActive(false);
            
            var currentText = value.GetSerializedValue();
            var blinker = true;
            var startTime = Time.unscaledTime;
            var keyboard = VirtualKeyboardProvider.Keyboard.Show(new() {
                initialText = currentText,
                updateCallback = update => {
                    if (currentText.Length > update.text.Length) {
                        AudioManager.Instance.PlayAudioEvent(Sfx.RemoveCharacter);
                    } else if (currentText.Length < update.text.Length) {
                        AudioManager.Instance.PlayAudioEvent(Sfx.AddCharacter);
                    }
                    currentText = update.text;
                    SetText(currentText, blinker: blinker);
                }
            });
            
            while(!keyboard.IsCompleted) {
                blinker = (Time.unscaledTime - startTime) % 1.5f < 0.75f;
                SetText(currentText, blinker: blinker);
                await GlobalTimer.NextTick();
            }
            
            SetText(currentText, blinker: false);
            InputController?.IsInputDisabled = false;
            button._selectedIndicator.SetActive(true);
            button.SetSubmitted(false);
            
            if(keyboard.Result.status == IVirtualKeyboard.ResultStatus.Confirmed) {
                value.SetSerializedValue(currentText);
                Log.Info($"Updated config [{key.Section}.{key.Key}] to \"{keyboard.Result.text}\".");
                AudioManager.Instance.PlayAudioEvent(Sfx.Confirm);
            } else {
                AudioManager.Instance.PlayAudioEvent(Sfx.Cancel);
            }
        };
        
        Descriptions[button] = value.Description.Description;
        
        var footer = AddPadding(plugin);
        
        return header; // this is cursed
    }
    
    public bool DeleteModMenu(PluginInfo plugin) {
        if(!ModMenus.TryGetValue(plugin, out var menu)) {
            return false;
        }
        OptionsGroup?.RemoveOption(menu.Button);
        Destroy(menu.Button.gameObject);
        Destroy(menu.Menu.gameObject);
        ModMenus.Remove(plugin);
        return true;
    }
    
    public void Awake() {
        if(!Initialized) {
            Log.Error($"{nameof(RiftModsSettingsController)} should be created using static {nameof(Create)} method.");
            Destroy(this);
            return;
        }
        
        (UnityEngine.Object, string)[] nullChecks = [
            (Template.Template, "Failed to load prefab for mod menu. This is usually loaded by cloning the accessibility settings menu."),
            (OptionsGroup, "Failed to initialize options group. This is usually initialized by cloning the options group from the accessibility settings menu."),
            (OptionsObj, "Failed to initialize options object. This is usually initialized by cloning the options object from the accessibility settings menu."),
            (InputController, "Failed to initialize input controller. This is usually initialized by cloning the input controller from the accessibility settings menu."),
            (BackButton, "Failed to initialize back button. This is usually initialized by cloning the back button from the accessibility settings menu."),
            (Template.TextButton, "Failed to load prefab for text buttons. This is usually loaded by cloning the button for the accessibility settings menu."),
            (Template.ToggleOption, "Failed to load prefab for toggle options. This is usually loaded by cloning a toggle option from the accessibility settings menu."),
            (Template.CarouselOptionGroup, "Failed to load prefab for carousel options. This is usually loaded by cloning a carousel option from the accessibility settings menu."),
            (Template.SliderOption, "Failed to load prefab for slider options. This is usually loaded by cloning a slider option from the audio settings menu."),
            (Template.BackButton, "Failed to load prefab for back button. This is usually loaded by cloning the back button from the accessibility settings menu."),
            (DescriptionLabel, "Failed to initialize description label. This is usually initialized by cloning the button for the accessibility settings menu."),
        ];
        
        foreach(var (obj, message) in nullChecks) {
            if(!obj) {
                Log.Fatal(message);
                Destroy(this);
                return;
            }
        }
        
        InputController?.OnCloseInput += HandleCloseInput;
    }
    
    public void Update() {
        if(!Initialized) {
            return;
        }
        
        var text = "";
        if(OptionsGroup.IsSelected) {
            var index = OptionsGroup._selectionIndex;
            if(0 <= index && index < OptionsGroup._options.Count) {
                Descriptions.TryGetValue(OptionsGroup._options[index], out text);
            }
        }
        
        Util.ForceSetText(DescriptionLabel._textLabels[0], text);
    }
    
    public void OnDestroy() {
        InputController?.OnCloseInput -= HandleCloseInput;
        OptionsGroup?.RemoveAllOptions(true);
        Destroy(OptionsGroup);
        Destroy(OptionsObj);
        Destroy(InputController);
        Destroy(BackButton);
        Destroy(DescriptionLabel);
        
        foreach(var menu in ModMenus.Values) {
            Destroy(menu.Menu);
        }
    }
    
    public void OnEnable() {
        OptionsObj?.SetActive(true);
        InputController?.Pipe(x => x.IsInputDisabled = false);
        InputController?.SetSelectionIndex(0);
        OptionsGroup?.SetSelectionIndex(0);
    }
    
    public void OnDisable() {
        OptionsObj?.SetActive(false);
        InputController?.Pipe(x => x.IsInputDisabled = true);
    }
    
    public void HandleCloseInput() {
        OnClose?.Invoke();
        InputController?.SetSelectionIndex(0);
    }
    
    public void PlayCancelSfx() {
        Sfx.Play(Sfx.Cancel);
    }
}
