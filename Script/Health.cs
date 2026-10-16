using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    private Vector3 spawnPoint;

    public float health = 50;
    public float MaxHealth = 50f;
    public bool isPlayer = false;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthtext;
    public void Start()
    {
        spawnPoint = transform.position;
       if(healthSlider !=null)
       {
       healthSlider.maxValue = MaxHealth;
       healthSlider.value = health;
       }
       updatetext();
    }
    public void Takedamage(float amount)
    {
        health -= amount;
        if(healthSlider !=null)
        healthSlider.value = health;
        if(health <= 0f)
        {
            Die();
        }
        updatetext();
    }
    void updatetext()
    {
        if(healthtext !=null)
        healthtext.text = Mathf.CeilToInt(health) + "/"+ Mathf.CeilToInt(MaxHealth);
    }
    public void Die()
    {
        if (isPlayer)
        {
            Invoke("Respawn", 2f);
        }
        else
        {
        Destroy(gameObject);
        }
    }
    void Respawn()
    {
        health = MaxHealth;
        transform.position = spawnPoint;
        if(healthSlider != null)
        healthSlider.value = health;
        updatetext();
    }
    void LateUpdate()
    {
    if(!isPlayer)
    {
    healthtext.transform.rotation = Camera.main.transform.rotation;
    healthSlider.transform.rotation = Camera.main.transform.rotation;
    }
    }   
}
