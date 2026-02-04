using UnityEngine;

public class BoostLimb : Limb
{
    public float boostAcceleration;
    public float duration;
    public float cooldown;
    private float timer;

    public Vector2 boostDirection;

    private bool isBoosting;

    protected override void Update()
    {
        timer = Mathf.Max(timer - Time.deltaTime, 0.0f);

        if (isBoosting)
        {
            if (timer > 0.0f)
            {
                Debug.Log(boostAcceleration * Time.deltaTime * (Vector2)transform.TransformDirection(boostDirection).normalized);
                body.velocity += boostAcceleration * Time.deltaTime * (Vector2)transform.TransformDirection(boostDirection).normalized;
                return;
            }

            timer = cooldown;
            isBoosting = false;
        }
    }

    public override void SecondaryAction()
    {
        if (isBoosting) return;

        if (timer > 0.0) return;

        timer = duration;
        isBoosting = true;
    }
}
