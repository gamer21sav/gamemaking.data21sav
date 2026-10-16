using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float health = 50f;
    public float MaxHealth = 50f;
    public bool isPlayer = false;
    public float respawnDelay = 2f;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthtext;

    private Vector3 spawnPoint;
    private bool isDead = false;

    void Start()
    {
        spawnPoint = transform.position;

        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = health;
        }
        UpdateText();
    }

    public void Takedamage(float amount)
    {
        if (isDead) return;

        health -= amount;

        if (healthSlider != null)
            healthSlider.value = health;

        UpdateText();

        if (health <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        health = Mathf.Max(health, 0f);

        // Shows the call chain so you can see which script dealt the killing blow
        Debug.Log($"{name} died. Call stack:\n{Environment.StackTrace}", this);

        if (isPlayer)
        {
            CancelInvoke(nameof(Respawn));
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Respawn()
    {
        // The CharacterController must be off while moving the transform
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = spawnPoint;

        if (cc != null) cc.enabled = true;

        health = MaxHealth;
        isDead = false;

        if (healthSlider != null)
            healthSlider.value = health;

        UpdateText();
    }

    void UpdateText()
    {
        if (healthtext != null)
            healthtext.text = Mathf.CeilToInt(health) + "/" + Mathf.CeilToInt(MaxHealth);
    }

    void LateUpdate()
    {
        if (isPlayer || Camera.main == null) return;

        if (healthtext != null)
            healthtext.transform.rotation = Camera.main.transform.rotation;

        if (healthSlider != null)
            healthSlider.transform.rotation = Camera.main.transform.rotation;
    }
}