using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selected = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectweapon();
    }

    // Update is called once per frame
    void Update()
    {
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
            newWeapon.gameObject.SetActive(false);
        }
    }
}
