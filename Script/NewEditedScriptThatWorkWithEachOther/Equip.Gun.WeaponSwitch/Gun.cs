using UnityEngine;

public class Gun : MonoBehaviour
{
    
    public float damage = 10f;
    public float range = 100f;
    public ParticleSystem muzzflash;
    public GameObject impacteffect;
    public float impactforce;
    public float firerate;
    private float firedelay = 0;
    private Transform aim;
    bool isLocalPlayer => GetComponentInParent<SimpleMove>()?.isLocalPlayer ?? false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer && Input.GetButton("Fire1") && Time.time >= firedelay)
        {
            firedelay = Time.time + 1/firerate;
            shoot();
        }
    }
        void shoot()
{
    if (muzzflash != null) muzzflash.Play();

    Transform eye = aim != null ? aim : (Camera.main != null ? Camera.main.transform : null);
    if (eye == null) return;

    RaycastHit hit;
    if (Physics.Raycast(eye.position, eye.forward, out hit, range))
    {
        // Don't damage the bot that is holding this weapon
        if (hit.transform.root == transform.root) return;

        Health target = hit.transform.GetComponent<Health>();
        if (target != null) target.Takedamage(damage);

        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForce(-hit.normal * impactforce);
        }

        if (impacteffect != null)
        {
            GameObject impactGo = Instantiate(impacteffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGo, 0.5f);
        }
    }
}
    public void SetAim(Transform t)
    {
        aim = t;
    }
    public void Activate()
    {
        this.enabled = true;
    }
    public void Deactivate()
    {
    this.enabled = false;
    }
}
