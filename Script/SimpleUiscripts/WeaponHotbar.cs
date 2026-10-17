using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHotbar : MonoBehaviour
{
    // ─── PUBLIC SETTINGS ────────────────────────────────

    public WeaponSwitching weaponSwitching;   // the Weapon Holder
    public Color normalColor = new Color(0f, 0f, 0f, 0.5f);
    public Color selectedColor = new Color(1f, 0.8f, 0.1f, 0.85f);

    // ─── PRIVATE STATE ──────────────────────────────────

    private Image[] slots = new Image[0];
    private TextMeshProUGUI[] labels = new TextMeshProUGUI[0];

    // ─── UPDATE LOOP ────────────────────────────────────

    void Update()
    {
        if (weaponSwitching == null) return;

        int count = weaponSwitching.transform.childCount;
        if (slots.Length != count)
            Rebuild(count);

        for (int i = 0; i < count; i++)
        {
            Transform weapon = weaponSwitching.transform.GetChild(i);
            labels[i].text = (i + 1) + ". " + weapon.name;
            slots[i].color = i == weaponSwitching.selected ? selectedColor : normalColor;
        }
    }

    // ─── HELPERS ────────────────────────────────────────

    void Rebuild(int count)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        slots = new Image[count];
        labels = new TextMeshProUGUI[count];

        for (int i = 0; i < count; i++)
        {
            // Box for this weapon
            GameObject slot = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            slot.transform.SetParent(transform, false);

            LayoutElement layout = slot.GetComponent<LayoutElement>();
            layout.preferredWidth = 120f;
            layout.preferredHeight = 50f;

            slots[i] = slot.GetComponent<Image>();

            // Text inside the box, stretched to fill it
            GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(slot.transform, false);

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18;

            labels[i] = tmp;
        }
    }
}