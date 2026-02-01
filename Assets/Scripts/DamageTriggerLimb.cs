using UnityEngine;

using System.Collections.Generic;

public class DamageTriggerLimb : Limb
{
    [Space]

    [HideInInspector] public Animator animator;

    public bool armed = true;

    public int damage;

    public float cooldown;
    private float cooldownTimer;

    public float hitKnockback;

    private List<Health> hitBlacklist = new List<Health>();

    protected override void Start()
    {
        base.Start();

        TryGetComponent(out animator);
    }

    protected override void Update()
    {
        // Update cooldown timer
        cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0.0f);

        // Clear blacklist when cooldown ends
        if (cooldownTimer == 0.0f)
        {
            hitBlacklist.Clear();
        }
    }

    private void OnTriggerStay2D(Collider2D collider2D)
    {
        if (!body.IsMasked()) return;

        if (!armed) return;

        // Get Rigidbody2D of other object
        Rigidbody2D otherRb = collider2D.attachedRigidbody;

        if (!otherRb) return;

        // Check for Health component
        Health otherHealth = otherRb.GetComponent<Health>();

        if (!otherHealth) return;

        // Don't damage teammates
        if (otherHealth.GetMaskTeam() == GetMaskTeam()) return;

        // Don't damage the same health component multiple times during cooldown
        if (hitBlacklist.Contains(otherHealth)) return;

        // Add to blacklist
        hitBlacklist.Add(otherHealth);

        // Deal damage
        otherHealth.TakeDamage(damage);

        // Apply knockback
        Vector2 knockbackDirection = (rb.position - otherRb.position).normalized;
        body.velocity = knockbackDirection * hitKnockback;

        // Start cooldown
        cooldownTimer = cooldown;
    }

    public override void PrimaryAction()
    {
        if (!body.IsMasked()) return;

        if (!animator) return;

        if (cooldownTimer > 0.0f) return;

        animator.SetTrigger("Attack");

        // Start cooldown
        cooldownTimer = cooldown;
    }
}
