using UnityEngine;

public class GunLimb : Limb
{
    public Vector2 shootOffset;

    public Projectile projectile;

    public float cooldown;
    private float cooldownTimer;

    protected override void Update()
    {
        // Update cooldown timer
        cooldownTimer = Mathf.Max(cooldownTimer - Time.deltaTime, 0.0f);
    }

    public override void PrimaryAction()
    {
        if (cooldownTimer > 0.0f) return;

        // Instantiate projectile
        Projectile newProjectile = Instantiate(projectile, transform.position + transform.TransformVector(shootOffset), transform.rotation);
        newProjectile.maskTeam = GetMaskTeam();

        // Reset cooldown timer
        cooldownTimer = cooldown;
    }
}
