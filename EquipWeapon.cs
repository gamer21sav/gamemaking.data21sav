using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private LayerMask weaponMask;
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;

    [Header("Drop")]
    [SerializeField] private KeyCode dropKey = KeyCode.G;
    [SerializeField] private float dropDistance = 1f;   // how far in front of the player
    [SerializeField] private float throwForce = 2f;     // small forward push when dropped

    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponSwitching weaponSwitching;

    private Camera cam;
    private RotateWeapon currentWeapon;
    private Gun currentGun;

    void Awake()
    {
        cam = Camera.main;
        if (weaponSwitching == null && weaponHolder != null)
            weaponSwitching = weaponHolder.GetComponent<WeaponSwitching>();
    }

    void Update()
    {
        if (Input.GetKeyDown(pickupKey)) TryPickup();
        if (Input.GetKeyDown(dropKey)) Drop();
    }

    // ─── PICKUP ─────────────────────────────────────────

    void TryPickup()
    {
        if (cam == null || weaponHolder == null) { Debug.LogError("EquipWeapon: check Main Camera and Weapon Holder."); return; }

        LayerMask mask = weaponMask.value == 0 ? ~0 : weaponMask;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, mask, QueryTriggerInteraction.Collide)) return;

        RotateWeapon weapon = hit.collider.GetComponentInParent<RotateWeapon>();
        if (weapon != null) Equip(weapon);
    }

    void Equip(RotateWeapon weapon)
    {
        currentWeapon = weapon;
        Transform t = weapon.transform;

        // Stop physics while held so the weapon doesn't push the player
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        SetCollidersEnabled(t, false);

        t.SetParent(weaponHolder, false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        weapon.IsRotating = false;

        currentGun = weapon.GetComponent<Gun>();
        if (currentGun != null) currentGun.Activate();
        if (weaponSwitching != null) weaponSwitching.OnWeaponPickedUp(t);
    }

    // ─── DROP ───────────────────────────────────────────

    void Drop()
    {
        // Drop the weapon that is actually in hand
        if (weaponSwitching != null && weaponSwitching.ActiveWeapon != null)
            currentWeapon = weaponSwitching.ActiveWeapon.GetComponent<RotateWeapon>();
        if (currentWeapon == null) return;

        Transform t = currentWeapon.transform;
        if (currentWeapon.TryGetComponent(out Gun gun)) gun.Deactivate();

        // Detach, place in front of the player, and make it visible again
        t.SetParent(null, true);
        t.position = transform.position + transform.forward * dropDistance + Vector3.up * 0.5f;
        t.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        t.gameObject.SetActive(true);

        // Turn physics back on and throw it forward
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb == null) rb = t.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = cam != null ? cam.transform.forward * throwForce : Vector3.zero;

        SetCollidersEnabled(t, true);
        currentWeapon.IsRotating = true;
        currentWeapon = null;

        if (weaponSwitching != null) weaponSwitching.OnWeaponDropped();
    }

    // ─── HELPERS ────────────────────────────────────────

    static void SetCollidersEnabled(Transform root, bool enabled)
    {
        foreach (Collider c in root.GetComponentsInChildren<Collider>(true)) c.enabled = enabled;
    }
}