using UnityEngine;

public class GunLimb : Limb
{
    public Vector2 shootOffset;

    public Projectile projectile;

    public float cooldown;
    private float cooldownTimer;

    private bool shooting = false;

    [Space]

    public string shootSound;

    protected override void Update()
    {
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
        if (cooldownTimer > 0.0f) return;

        shooting = true;

        // Instantiate projectile
        Projectile newProjectile = Instantiate(projectile, transform.position + transform.TransformVector(shootOffset), transform.rotation);
        newProjectile.maskTeam = GetMaskTeam();

        // Play shoot sound
        SoundManager.instance.PlaySound(shootSound);

        // Reset cooldown timer
        cooldownTimer = cooldown;
    }

    public override void PrimaryActionEnd()
    {
        shooting = false;
    }
}
