using UnityEngine;

public class RotateWeapon : MonoBehaviour
{
    private Rigidbody weaponbody;
    public bool IsRotating { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponbody = GetComponent<Rigidbody>();
        if (weaponbody)
        {
            weaponbody.isKinematic = true;
        }
        IsRotating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsRotating){return;}
        transform.Rotate(Vector3.up * Time.deltaTime * 50f);

    }

}
