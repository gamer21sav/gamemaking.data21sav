
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("References")]
    public Camera playerCamera;
    public AudioMixer audioMixer;

    [Header("Audio Mixer Exposed Parameters")]
    public string masterVolumeParameter = "MasterVolume";
    public string musicVolumeParameter = "MusicVolume";
    public string sfxVolumeParameter = "SFXVolume";

    [Header("Gameplay Defaults")]
    [Range(40f, 120f)] public float defaultFOV = 90f;
    [Range(0.1f, 10f)] public float defaultSensitivityX = 2f;
    [Range(0.1f, 10f)] public float defaultSensitivityY = 2f;
    [Range(0.1f, 1f)] public float defaultAimSensitivity = 0.5f;

    [Header("Current Settings")]
    [Range(40f, 120f)] public float fov = 90f;
    [Range(0.1f, 10f)] public float sensitivityX = 2f;
    [Range(0.1f, 10f)] public float sensitivityY = 2f;
    [Range(0.1f, 1f)] public float aimSensitivity = 0.5f;
    public bool invertY;

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public int resolutionIndex;
    public bool fullscreen = true;

    // Custom key bindings
    public Dictionary<string, KeyCode> keys =
        new Dictionary<string, KeyCode>();

    public static readonly string[] BindingNames =
    {
        "Forward", "Backward", "Left", "Right",
        "Jump", "Sprint", "Aim", "Fire", "Pause"
    };

    readonly KeyCode[] defaultKeys =
    {
        KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
        KeyCode.Space, KeyCode.LeftShift, KeyCode.Mouse1,
        KeyCode.Mouse0, KeyCode.Escape
    };

    const string Prefix = "GameSettings_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAll();
    }

    public KeyCode GetKey(string action)
    {
        if (keys.TryGetValue(action, out KeyCode key))
            return key;

        return KeyCode.None;
    }

    public void SetKey(string action, KeyCode key)
    {
        keys[action] = key;
        PlayerPrefs.SetInt(Prefix + "Key_" + action, (int)key);
        PlayerPrefs.Save();
    }

    public void SetFOV(float value)
    {
        fov = Mathf.Clamp(value, 40f, 120f);
        if (playerCamera != null)
            playerCamera.fieldOfView = fov;
        SaveFloat("FOV", fov);
    }

    public void SetSensitivityX(float value)
    {
        sensitivityX = Mathf.Clamp(value, 0.1f, 10f);
        SaveFloat("SensitivityX", sensitivityX);
    }

    public void SetSensitivityY(float value)
    {
        sensitivityY = Mathf.Clamp(value, 0.1f, 10f);
        SaveFloat("SensitivityY", sensitivityY);
    }

    public void SetAimSensitivity(float value)
    {
        aimSensitivity = Mathf.Clamp(value, 0.1f, 1f);
        SaveFloat("AimSensitivity", aimSensitivity);
    }

    public void SetInvertY(bool value)
    {
        invertY = value;
        PlayerPrefs.SetInt(Prefix + "InvertY", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyAudio();
        SaveFloat("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyAudio();
        SaveFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyAudio();
        SaveFloat("SFXVolume", sfxVolume);
    }

    public void SetFullscreen(bool value)
    {
        fullscreen = value;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(Prefix + "Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int index)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutions.Length == 0) return;

        resolutionIndex = Mathf.Clamp(index, 0, resolutions.Length - 1);
        Resolution r = resolutions[resolutionIndex];

        Screen.SetResolution(r.width, r.height, fullscreen);

        PlayerPrefs.SetInt(Prefix + "Resolution", resolutionIndex);
        PlayerPrefs.Save();
    }

    public Resolution[] GetResolutions()
    {
        return Screen.resolutions;
    }

    public void ApplyAll()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = fov;

        Screen.fullScreen = fullscreen;

        Resolution[] resolutions = Screen.resolutions;
        if (resolutions.Length > 0)
        {
            resolutionIndex = Mathf.Clamp(
                resolutionIndex, 0, resolutions.Length - 1);

            Resolution r = resolutions[resolutionIndex];
            Screen.SetResolution(r.width, r.height, fullscreen);
        }

        ApplyAudio();
    }

    void ApplyAudio()
    {
        if (audioMixer == null) return;

        SetMixerVolume(masterVolumeParameter, masterVolume);
        SetMixerVolume(musicVolumeParameter, musicVolume);
        SetMixerVolume(sfxVolumeParameter, sfxVolume);
    }

    void SetMixerVolume(string parameter, float value)
    {
        // Mixer volume is measured in decibels.
        float db = value <= 0.0001f
            ? -80f
            : Mathf.Log10(value) * 20f;

        audioMixer.SetFloat(parameter, db);
    }

    void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(Prefix + key, value);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        fov = PlayerPrefs.GetFloat(Prefix + "FOV", defaultFOV);
        sensitivityX = PlayerPrefs.GetFloat(
            Prefix + "SensitivityX", defaultSensitivityX);
        sensitivityY = PlayerPrefs.GetFloat(
            Prefix + "SensitivityY", defaultSensitivityY);
        aimSensitivity = PlayerPrefs.GetFloat(
            Prefix + "AimSensitivity", defaultAimSensitivity);

        invertY = PlayerPrefs.GetInt(Prefix + "InvertY", 0) == 1;

        masterVolume = PlayerPrefs.GetFloat(Prefix + "MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat(Prefix + "MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat(Prefix + "SFXVolume", 1f);

        fullscreen = PlayerPrefs.GetInt(Prefix + "Fullscreen", 1) == 1;
        resolutionIndex = PlayerPrefs.GetInt(Prefix + "Resolution", 0);

        for (int i = 0; i < BindingNames.Length; i++)
        {
            string action = BindingNames[i];
            int savedKey = PlayerPrefs.GetInt(
                Prefix + "Key_" + action, (int)defaultKeys[i]);

            keys[action] = (KeyCode)savedKey;
        }
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteKey(Prefix + "FOV");
        PlayerPrefs.DeleteKey(Prefix + "SensitivityX");
        PlayerPrefs.DeleteKey(Prefix + "SensitivityY");
        PlayerPrefs.DeleteKey(Prefix + "AimSensitivity");
        PlayerPrefs.DeleteKey(Prefix + "InvertY");
        PlayerPrefs.DeleteKey(Prefix + "MasterVolume");
        PlayerPrefs.DeleteKey(Prefix + "MusicVolume");
        PlayerPrefs.DeleteKey(Prefix + "SFXVolume");
        PlayerPrefs.DeleteKey(Prefix + "Fullscreen");
        PlayerPrefs.DeleteKey(Prefix + "Resolution");

        foreach (string action in BindingNames)
            PlayerPrefs.DeleteKey(Prefix + "Key_" + action);

        PlayerPrefs.Save();

        LoadSettings();
        ApplyAll();
    }
}
