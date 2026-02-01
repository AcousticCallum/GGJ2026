using UnityEngine;

public class MovementLimb : Limb
{
    [HideInInspector] public Animator animator;

    public float minimumAnimSpeed;
    public float speedMultiplier;

    public float animationOffset;

    protected override void Start()
    {
        base.Start();

        TryGetComponent(out animator);

        animator.SetFloat("Offset", animationOffset);
    }

    protected override void Update()
    {
        float speed = minimumAnimSpeed + body.velocity.magnitude * speedMultiplier;

        animator.SetFloat("Speed", speed);
    }
}
