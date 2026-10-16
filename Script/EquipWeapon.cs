using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
    private RaycastHit Hitinfo;
    [SerializeField]private LayerMask weaponMask;
    private RotateWeapon currentWeapon;
    private Gun gun;
    [SerializeField] private Transform weaponHolder;
    [SerializeField]private WeaponSwitching weaponSwitching;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponSwitching = weaponHolder.GetComponent<WeaponSwitching>();
    }
    private void Update()
    {
        rayhandler();
        if (Input.GetKeyDown(KeyCode.F))
        {
            Equip();
        }
    }

    // Update is called once per frame

    void rayhandler()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out Hitinfo, 2.5f,weaponMask))
        {
            Debug.DrawLine(ray.origin, Hitinfo.point, Color.red);
        }
    }
    void Equip()
    {
        currentWeapon = null;
        if(Hitinfo.collider != null)
        {
            currentWeapon = Hitinfo.collider.GetComponent<RotateWeapon>();
        }
        if(!currentWeapon)
        {
            return;
        }
        currentWeapon.transform.SetParent(weaponHolder, false);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = Vector3.one;

        currentWeapon.IsRotating = false;
        weaponSwitching.OnWeaponPickedUp(currentWeapon.transform);
        gun = currentWeapon.GetComponent<Gun>();
        if(gun != null) gun.Activate();
    }
}
