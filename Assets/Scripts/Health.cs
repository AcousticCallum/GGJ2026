using UnityEngine;

using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent onDeath;

    [HideInInspector] public int maxHealth;
    public int health;

    public float damageCooldown;
    private float damageCooldownTimer;

    public bool dead;

    private void Update()
    {
        damageCooldownTimer = Mathf.Max(damageCooldownTimer - Time.deltaTime, 0.0f);
    }

    public void TakeDamage(int damage)
    {
        if (!CanDamage() || dead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (dead) return;
        dead = true;

        onDeath.Invoke();
    }

    public bool CanDamage()
    {
        return damageCooldownTimer <= 0.0f;
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
}
