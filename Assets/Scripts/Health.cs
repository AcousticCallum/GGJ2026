using UnityEngine;

using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent onDeath;

    [HideInInspector] public int maxHealth;
    public int health;

    public float damageCooldown;
    private float damageCooldownTimer;

    public float damageKnockbackMultiplier = 1.0f;

    public bool dead;
    public bool invulnerable;

    [Space]

    public string damageSound;
    public string deathSound;

    [Space]

    public ParticleSystem hitEffect;

    private static float damageSlowDownTimer;
    private static bool damageSlowDownUpdated;

    private void Start()
    {
        maxHealth = health;
    }

    private void Update()
    {
        damageCooldownTimer = Mathf.Max(damageCooldownTimer - Time.deltaTime, 0.0f);

        if (!damageSlowDownUpdated)
        {
            damageSlowDownTimer = Mathf.Max(damageSlowDownTimer - Time.unscaledDeltaTime, 0.0f);
            if (damageSlowDownTimer == 0.0f)
            {
                Time.timeScale = 1.0f;
            }

            damageSlowDownUpdated = true;
        }
    }

    private void LateUpdate()
    {
        damageSlowDownUpdated = false;
    }

    public void TakeDamage(int damage)
    {
        if (!CanDamage()) return;

        // Play damage sound
        SoundManager.instance.PlaySound(damageSound);

        if (invulnerable) return;

        if (dead) return;
        
        health -= damage;

        damageSlowDownTimer = 0.25f;
        Time.timeScale = 0.5f;

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (invulnerable) return;

        if (dead) return;
        dead = true;

        // Play death sound
        SoundManager.instance.PlaySound(deathSound);

        onDeath.Invoke();
    }

    public bool CanDamage()
    {
        return damageCooldownTimer == 0.0f;
    }

    public void ResetHealth()
    {
        health = maxHealth;
        dead = false;
    }

    public Mask.MaskTeam GetMaskTeam()
    {
        if (TryGetComponent(out Mask mask)) return mask.maskTeam;

        if (TryGetComponent(out Limb limb)) return limb.GetMaskTeam();

        if (TryGetComponent(out Body body)) return body.GetMaskTeam();

        return Mask.MaskTeam.Neutral;
    }

    public void SpawnHitEffect(Vector2 position)
    {
        if (hitEffect)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
    }
}
