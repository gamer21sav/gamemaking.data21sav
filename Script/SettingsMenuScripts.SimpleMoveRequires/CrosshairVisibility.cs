using UnityEngine;
using UnityEngine.UI;

public class CrosshairVisibility : MonoBehaviour
{
    RawImage crosshairImage;

    void Awake()
    {
        crosshairImage = GetComponent<RawImage>();
    }

    void Update()
    {
        crosshairImage.enabled = !SettingsUI.IsOpen;
    }
}
