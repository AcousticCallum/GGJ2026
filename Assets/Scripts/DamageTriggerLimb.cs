using UnityEngine;

using System.Collections.Generic;

public class DamageTriggerLimb : Limb
{
    [Space]

    [HideInInspector] public Animator animator;
    private bool attacking;

    public ArmLimb connectedArm;

    public bool armed = true;

    public int damage;

    public float cooldown;
    private float cooldownTimer;

    public float actionCooldown;
    private float actionCooldownTimer;

    public float knockback;
    public float recoilKnockback;
    public bool knockbackWhenUnarmed;
    public float knockbackStunDuration;

    private List<Health> hitBlacklist = new List<Health>();

    protected override void Start()
    {
        base.Start();

        TryGetComponent(out animator);
    }

    protected override void Update()
    {
        if(!body.IsMasked()) return;

        // Update cooldown timers
        cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0.0f);
        actionCooldownTimer = Mathf.Max(actionCooldownTimer - Time.deltaTime, 0.0f);

        // Clear blacklist when cooldown ends
        if (cooldownTimer == 0.0f)
        {
            hitBlacklist.Clear();
        }

        // Try action
        AttackAction();
    }

    private void OnTriggerStay2D(Collider2D collider2D)
    {
        if (!body.IsMasked()) return;

        if (!armed && !knockbackWhenUnarmed) return;

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
        if (armed) otherHealth.TakeDamage(damage);

        // Calculate knockback
        Vector2 newKnockback = otherHealth.damageKnockbackMultiplier * recoilKnockback * (rb.position - collider2D.ClosestPoint(rb.position)).normalized;

        // Apply knockback to connected arm first
        if (connectedArm)
        {
            // Apply knockback to connected arm
            connectedArm.GiveKnockback(newKnockback);

            // Stun connected arm
            connectedArm.Stun(knockbackStunDuration);

            // Reduce knockback based on connected arm's knockback ratio
            newKnockback *= 1.0f - connectedArm.knockbackRatio;
        }

        // Apply knockback to body
        if (body.knockbackResistance < 0)
        {
            body.velocity = (1.0f - body.knockbackResistance) * newKnockback;
        }
        else
        {
            body.velocity = Vector2.Lerp(newKnockback, body.velocity, body.knockbackResistance);
        }

        // Start cooldown
        cooldownTimer = cooldown;
    }

    protected override void OnAdd()
    {
        base.OnAdd();

        attacking = false;
    }

    public override void PrimaryAction()
    {
        if (!body.IsMasked()) return;

        attacking = true;
    }

    public override void PrimaryActionEnd()
    {
        if (!body.IsMasked()) return;

        attacking = false;
    }

    // Do an attack
    private void AttackAction()
    {
        if (!attacking) return;

        if (!body.IsMasked()) return;

        if (!animator) return;

        if (actionCooldownTimer > 0.0f) return;

        animator.SetTrigger("Attack");

        // Start cooldown
        actionCooldownTimer = actionCooldown;
    }
}
