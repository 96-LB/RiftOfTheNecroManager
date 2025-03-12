using Shared.DLC;
using Shared.MenuOptions;
using Shared.PlayerData;
using Shared.Title;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LalaDancer.Scripts;

public class RiftModsSettingsController : MonoBehaviour {
    [Header("Menu Buttons")]
    [SerializeField]
    private GameObject _contentParent;

    [SerializeField]
    private OptionsScreenInputController _inputController;

    [SerializeField]
    private List<TextButtonOption> modsButtons;

    [SerializeField]
    private TextButtonOption _backButton;

    private SettingsMenuManager _settingsMenuManager;

    public event Action OnClose;

    public event Action OnResetSettings;

    private void Awake() {
        if((bool)SettingsAccessor.Instance) {
            _settingsMenuManager = SettingsAccessor.Instance.RequestSettingsMenu();
        }

        if((bool)_inputController) {
            _inputController.OnCloseInput += OnCancel;
        }

        if((bool)_backButton) {
            _backButton.OnSubmit += OnCancel;
        }
    }

    private void OnDestroy() {
        if((bool)_inputController) {
            _inputController.OnCloseInput -= OnCancel;
        }

        if((bool)_backButton) {
            _backButton.OnSubmit -= OnCancel;
        }
    }

    private void OnEnable() {
        _contentParent.SetActive(value: true);
    }

    private void OnCancel() {
        HandleCloseInput();
    }

    private void HandleCloseInput() {
        OnClose?.Invoke();
        _inputController.SetSelectionIndex(0);
    }
}
