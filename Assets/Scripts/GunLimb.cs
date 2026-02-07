using UnityEngine;

public class GunLimb : Limb
{
    public Vector2 shootOffset;

    public Projectile projectile;

    public ArmLimb connectedArm;

    public float cooldown;
    private float cooldownTimer;

    private bool shooting = false;

    public float knockback;
    public float recoilKnockback;
    public float knockbackStunDuration;

    [Space]

    public string shootSound;

    protected override void Update()
    {
        if (!body.IsMasked()) return;

        // Update cooldown timer
        cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0.0f);

        if(shooting) PrimaryAction();
    }

    protected override void OnAdd()
    {
        base.OnAdd();

        shooting = false;
    }

    public override void PrimaryAction()
    {
        if (!body.IsMasked()) return;

        if (cooldownTimer > 0.0f) return;

        shooting = true;

        // Instantiate projectile
        Projectile newProjectile = Instantiate(projectile, transform.position + transform.TransformVector(shootOffset), transform.rotation);
        newProjectile.maskTeam = GetMaskTeam();

        // Calculate knockback
        Vector2 newKnockback = recoilKnockback * -transform.up;

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

        newKnockback *= 1.0f - body.knockbackResistance;

        if (newKnockback != Vector2.zero)
        {
            // Apply knockback to body
            float dot = Vector2.Dot(body.velocity, newKnockback.normalized);
            if (dot <= 1.0f)
            {
                body.velocity = body.velocity - (dot * newKnockback.normalized) + newKnockback;
            }
        }

        // Play shoot sound
        SoundManager.instance.PlaySound(shootSound);

        // Reset cooldown timer
        cooldownTimer = cooldown;
    }

    public override void PrimaryActionEnd()
    {
        if (!body.IsMasked()) return;

        shooting = false;
    }
}
