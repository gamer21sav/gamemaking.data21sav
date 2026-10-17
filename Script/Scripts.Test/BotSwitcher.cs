using UnityEngine;

public class BotSwitcher : MonoBehaviour
{
    public KeyCode switchKey = KeyCode.Tab;

    SimpleMove[] bots;
    int index = 0;

    void Start()
    {
        bots = FindObjectsByType<SimpleMove>(FindObjectsSortMode.None);
        ApplyAll();
    }

    void Update()
    {
        if (bots == null || bots.Length == 0) return;
        if (!Input.GetKeyDown(switchKey)) return;

        index = (index + 1) % bots.Length;
        ApplyAll();
    }

    void ApplyAll()
    {
        for (int i = 0; i < bots.Length; i++)
        {
            bots[i].isLocalPlayer = i == index;
            bots[i].ApplyLocalState();
        }
    }
}