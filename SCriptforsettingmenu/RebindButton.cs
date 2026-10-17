
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RebindButton : MonoBehaviour
{
    [Header("Binding")]
    public string actionName = "Jump";
    public Button button;
    public TMP_Text label;

    bool listening;
    GameSettings settings;

    void Start()
    {
        settings = GameSettings.Instance;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(StartRebinding);

        RefreshLabel();
    }

    public void StartRebinding()
    {
        if (listening) return;

        StartCoroutine(WaitForKey());
    }

    IEnumerator WaitForKey()
    {
        listening = true;

        if (label != null)
            label.text = "Press a key...";

        // Avoid reading the click that activated the button.
        yield return null;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RefreshLabel();
                listening = false;
                yield break;
            }

            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (key == KeyCode.None) continue;

                if (Input.GetKeyDown(key))
                {
                    settings.SetKey(actionName, key);
                    RefreshLabel();
                    listening = false;
                    yield break;
                }
            }

            yield return null;
        }
    }

    void RefreshLabel()
    {
        if (settings == null)
            settings = GameSettings.Instance;

        if (settings == null || label == null) return;

        label.text = settings.GetKey(actionName).ToString();
    }
}
