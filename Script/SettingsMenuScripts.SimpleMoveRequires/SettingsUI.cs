
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [Header("Main UI")]
    public GameObject settingsRoot;
    public GameObject mainPanel;
    public GameObject gameplayPanel;
    public GameObject audioPanel;
    public GameObject controlsPanel;
    public GameObject displayPanel;

    [Header("Gameplay Sliders")]
    public Slider fovSlider;
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Slider aimSensitivitySlider;
    public Toggle invertYToggle;

    [Header("Gameplay Value Labels")]
    public TMP_Text fovValue;
    public TMP_Text sensitivityXValue;
    public TMP_Text sensitivityYValue;
    public TMP_Text aimSensitivityValue;

    [Header("Audio Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Audio Value Labels")]
    public TMP_Text masterVolumeValue;
    public TMP_Text musicVolumeValue;
    public TMP_Text sfxVolumeValue;

    [Header("Display")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    GameSettings settings;
    Resolution[] resolutions;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        settings = GameSettings.Instance;

        if (settings == null)
        {
            Debug.LogError("No GameSettings object exists in the scene.");
            return;
        }

        ConfigureSliders();
        PopulateResolutions();
        LoadUIFromSettings();

        ShowMainPanel();
        SetMenuOpen(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsOpen)
            {
                if (mainPanel != null && mainPanel.activeSelf)
                    SetMenuOpen(false);
                else
                    ShowMainPanel();
            }
            else
            {
                SetMenuOpen(true);
                ShowMainPanel();
            }
        }
    }

    void ConfigureSliders()
    {
        if (fovSlider != null)
        {
            fovSlider.minValue = 40f;
            fovSlider.maxValue = 120f;
            fovSlider.onValueChanged.AddListener(OnFOVChanged);
        }

        if (sensitivityXSlider != null)
        {
            sensitivityXSlider.minValue = 0.1f;
            sensitivityXSlider.maxValue = 10f;
            sensitivityXSlider.onValueChanged.AddListener(OnSensitivityXChanged);
        }

        if (sensitivityYSlider != null)
        {
            sensitivityYSlider.minValue = 0.1f;
            sensitivityYSlider.maxValue = 10f;
            sensitivityYSlider.onValueChanged.AddListener(OnSensitivityYChanged);
        }

        if (aimSensitivitySlider != null)
        {
            aimSensitivitySlider.minValue = 0.1f;
            aimSensitivitySlider.maxValue = 1f;
            aimSensitivitySlider.onValueChanged.AddListener(OnAimSensitivityChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    void LoadUIFromSettings()
    {
        fovSlider?.SetValueWithoutNotify(settings.fov);
        sensitivityXSlider?.SetValueWithoutNotify(settings.sensitivityX);
        sensitivityYSlider?.SetValueWithoutNotify(settings.sensitivityY);
        aimSensitivitySlider?.SetValueWithoutNotify(settings.aimSensitivity);

        invertYToggle?.SetIsOnWithoutNotify(settings.invertY);

        masterVolumeSlider?.SetValueWithoutNotify(settings.masterVolume);
        musicVolumeSlider?.SetValueWithoutNotify(settings.musicVolume);
        sfxVolumeSlider?.SetValueWithoutNotify(settings.sfxVolume);

        fullscreenToggle?.SetIsOnWithoutNotify(settings.fullscreen);

        UpdateValueLabels();
    }

    void PopulateResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutions = settings.GetResolutions();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution r = resolutions[i];
            options.Add($"{r.width} x {r.height} ({r.refreshRateRatio})");

            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        if (resolutions.Length > 0)
        {
            int saved = Mathf.Clamp(
                settings.resolutionIndex, 0, resolutions.Length - 1);

            resolutionDropdown.SetValueWithoutNotify(saved);
        }
    }

    public void OnFOVChanged(float value)
    {
        settings.SetFOV(value);
        UpdateValueLabels();
    }

    public void OnSensitivityXChanged(float value)
    {
        settings.SetSensitivityX(value);
        UpdateValueLabels();
    }

    public void OnSensitivityYChanged(float value)
    {
        settings.SetSensitivityY(value);
        UpdateValueLabels();
    }

    public void OnAimSensitivityChanged(float value)
    {
        settings.SetAimSensitivity(value);
        UpdateValueLabels();
    }

    public void OnInvertYChanged(bool value)
    {
        settings.SetInvertY(value);
    }

    public void OnMasterVolumeChanged(float value)
    {
        settings.SetMasterVolume(value);
        UpdateValueLabels();
    }

    public void OnMusicVolumeChanged(float value)
    {
        settings.SetMusicVolume(value);
        UpdateValueLabels();
    }

    public void OnSFXVolumeChanged(float value)
    {
        settings.SetSFXVolume(value);
        UpdateValueLabels();
    }

    public void OnFullscreenChanged(bool value)
    {
        settings.SetFullscreen(value);
    }

    public void OnResolutionChanged(int index)
    {
        settings.SetResolution(index);
    }

    void UpdateValueLabels()
    {
        SetLabel(fovValue, settings.fov.ToString("0"));
        SetLabel(sensitivityXValue, settings.sensitivityX.ToString("0.00"));
        SetLabel(sensitivityYValue, settings.sensitivityY.ToString("0.00"));
        SetLabel(aimSensitivityValue, settings.aimSensitivity.ToString("0.00"));

        SetLabel(masterVolumeValue, (settings.masterVolume * 100f).ToString("0") + "%");
        SetLabel(musicVolumeValue, (settings.musicVolume * 100f).ToString("0") + "%");
        SetLabel(sfxVolumeValue, (settings.sfxVolume * 100f).ToString("0") + "%");
    }

    void SetLabel(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }

    public void ShowMainPanel()
    {
        SetPanel(mainPanel);
    }

    public void ShowGameplayPanel()
    {
        SetPanel(gameplayPanel);
    }

    public void ShowAudioPanel()
    {
        SetPanel(audioPanel);
    }

    public void ShowControlsPanel()
    {
        SetPanel(controlsPanel);
    }

    public void ShowDisplayPanel()
    {
        SetPanel(displayPanel);
    }

    void SetPanel(GameObject selected)
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (displayPanel != null) displayPanel.SetActive(false);

        if (selected != null)
            selected.SetActive(true);
    }

    public void SetMenuOpen(bool open)
    {
        IsOpen = open;

        if (settingsRoot != null)
            settingsRoot.SetActive(open);

        Time.timeScale = open ? 0f : 1f;
        SimpleMove.SetCursor(!open);
    }

    public void CloseMenu()
    {
        SetMenuOpen(false);
    }

    public void ResetAllSettings()
    {
        settings.ResetSettings();
        LoadUIFromSettings();
        PopulateResolutions();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsOpen = false;
            Time.timeScale = 1f;
        }
    }
}
