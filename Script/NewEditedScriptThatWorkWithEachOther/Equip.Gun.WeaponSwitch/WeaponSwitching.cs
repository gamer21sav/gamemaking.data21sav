using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selected = 0;
    bool isLocalPlayer => GetComponentInParent<SimpleMove>()?.isLocalPlayer ?? false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectweapon();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        int previousselected = selected;
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {   
            if (selected >= transform.childCount -1)
            selected = 0;
            else
            selected++;
        }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {   
            if (selected <= 0)
            selected = transform.childCount-1;
            else
            selected--;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {selected = 0;}
        if(Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {selected = 1;}
        if(Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
        {selected = 2;}
        if(Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
        {selected = 3;}
        if(Input.GetKeyDown(KeyCode.Alpha5) && transform.childCount >= 5)
        {selected = 4;}
        if(Input.GetKeyDown(KeyCode.Alpha6) && transform.childCount >= 6)
        {selected = 5;}
if (previousselected != selected){selectweapon();}
    }
    void selectweapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selected)
            weapon.gameObject.SetActive(true);
            else
            weapon.gameObject.SetActive(false);
            i++;

        }
    }
    public void OnWeaponPickedUp(Transform newWeapon)
    {
        if(transform.childCount == 1)
        {
            selected = 0;
            selectweapon(); 
            
        }
        else
        {
            //NEW CODE ADDED
            selected = newWeapon.GetSiblingIndex();
            selectweapon();
        }
    }
    // NEW script CODE ADD
    public Transform ActiveWeapon
    {
        get
        {
            if (transform.childCount == 0) return null;
            return transform.GetChild(Mathf.Clamp(selected, 0, transform.childCount - 1));
        }
    }

    // Call after a weapon is removed from the holder so the selection stays valid
    public void OnWeaponDropped()
    {
        if (transform.childCount == 0) return;
        if (selected >= transform.childCount)
            selected = transform.childCount - 1;
        selectweapon();
    }
}
