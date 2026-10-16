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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("Fire1") && Time.time >= firedelay)
        {
            firedelay = Time.time + 1/firerate;
            shoot();
        }
    }
        void shoot()
    {
        muzzflash.Play();
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            Debug.Log(hit.transform.name);
            Health target = hit.transform.GetComponent<Health>();
            if (target != null){target.Takedamage(damage);}

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactforce);
            }

            GameObject impactGo = Instantiate(impacteffect,hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGo, 0.5f);
        }
    }
    public void Activate()
    {
        this.enabled = true;
    }
}
