using UnityEngine;

using System.Collections.Generic;

public class DamageTriggerLimb : Limb
{
    [Space]

    public int damage;

    public float cooldown;
    private float cooldownTimer;

    protected override void Update()
    {
        // Update cooldown timer
        cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0.0f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Check cooldown
        if (OnCooldown()) return;

        // Get Rigidbody2D of other object
        Rigidbody2D otherRb = collision.rigidbody;

        if (!otherRb) return;

        // Check for Health component
        Health otherHealth = otherRb.GetComponent<Health>();

        if (!otherHealth) return;

        // Don't damage teammates
        if (otherHealth.GetMaskTeam() == GetMaskTeam()) return;

        // Deal damage
        otherHealth.TakeDamage(damage);

        // Start cooldown
        cooldownTimer = cooldown;
    }

    public bool OnCooldown()
    {
        return cooldownTimer > 0.0f;
    }
}
