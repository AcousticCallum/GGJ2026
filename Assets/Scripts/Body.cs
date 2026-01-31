using UnityEngine;

using System.Collections.Generic;

public class Body : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;

    public Transform[] maskSlots;
    public List<Mask> masks;

    [Space]

    public Limb[] limbs;

    [Space]

    public float moveSpeed;
    public float moveAcceleration;
    public float rotateSpeed;

    private Vector2 targetVelocity;
    private float targetRotation;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float rotation;

    [Space]

    public float deathDelay;

    private bool dying;
    private float deathTimer;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        // Initialise stat bonuses
        float moveSpeedMultiplier = 1.0f;
        float moveAccelerationMultiplier = 1.0f;
        float rotateSpeedMultiplier = 1.0f;

        // Apply stat bonuses from limbs
        foreach (Limb limb in limbs)
        {
            // Skip destroyed limbs
            if (limb.destroyed) continue;

            // Apply each stat bonus
            foreach (StatBonus statBonus in limb.statBonuses)
            {
                switch (statBonus.statBonusType)
                {
                    case StatBonus.StatBonusType.moveSpeed:
                        moveSpeedMultiplier += statBonus.value;
                        break;

                    case StatBonus.StatBonusType.moveAcceleration:
                        moveAccelerationMultiplier += statBonus.value;
                        break;

                    case StatBonus.StatBonusType.rotateSpeed:
                        rotateSpeedMultiplier += statBonus.value;
                        break;
                }
            }
        }

        // Smoothly move towards target velocity and rotation
        velocity = Vector2.MoveTowards(velocity, moveSpeedMultiplier * targetVelocity, moveAccelerationMultiplier * moveAcceleration * Time.deltaTime);
        rotation = Mathf.LerpAngle(rotation, targetRotation, rotateSpeedMultiplier * rotateSpeed * Time.deltaTime);

        // Handle death timer
        if (dying)
        {
            deathTimer = Mathf.Max(deathTimer - Time.deltaTime, 0.0f);

            if (deathTimer == 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        // Apply movement and rotation to Rigidbody2D
        rb.MovePosition(rb.position + Time.fixedDeltaTime * velocity);
        rb.MoveRotation(rotation);
    }

    public void Move(Vector2 direction)
    {
        targetVelocity = moveSpeed * direction.normalized;
    }

    public void Rotate(float direction)
    {
        targetRotation = direction;
        //if (targetRotation - rotation > 180.0f) targetRotation -= 360.0f;
        //else if (targetRotation - rotation < -180.0f) targetRotation += 360.0f;
    }

    public void PrimaryAction()
    {
        foreach (Limb limb in limbs)
        {
            // Skip destroyed limbs
            if (limb.destroyed) continue;

            limb.PrimaryAction();
        }
    }

    public void SecondaryAction()
    {
        foreach (Limb limb in limbs)
        {
            // Skip destroyed limbs
            if (limb.destroyed) continue;

            limb.SecondaryAction();
        }
    }

    public virtual void AddMask(Mask maskPrefab, bool forceAdd = false)
    {
        // Body is already masked
        if (IsMasked())
        {
            // Give up if not forcing
            if (!forceAdd) return;

            // Remove current mask
            while (masks.Count > 0)
            {
                RemoveMask(masks[0]);
            }
        }

        foreach (Transform slot in maskSlots)
        {
            // Instantiate mask in the slot
            Mask mask = Instantiate(maskPrefab, slot);
            mask.body = this;

            // If this is the first mask, set it as the controller
            if (masks.Count == 0) mask.controller = true;

            // Add mask to list
            masks.Add(mask);
        }

        // Cancel death now that the body has a mask
        CancelDeath();
    }

    public virtual void RemoveMask(Mask mask)
    {
        // Mask not found
        if (!masks.Contains(mask)) return;

        // Remove mask from list
        masks.Remove(mask);

        // No masks left, remove mask and start death
        if (masks.Count == 0)
        {
            mask.OnRemove();
            StartDeath();

            return;
        }

        // If the removed mask was a controller, assign a new one
        if (mask.controller)
        {
            masks[0].controller = true;
        }

        // Remove the mask
        mask.OnRemove();
    }

    public bool IsMasked()
    {
        return masks.Count > 0;
    }

    public void StartDeath()
    {
        dying = true;
        deathTimer = deathDelay;

        targetVelocity = Vector2.zero;
        targetRotation = rotation;
    }

    public void CancelDeath(bool resetLimbs = true)
    {
        dying = false;
        deathTimer = 0.0f;

        if (resetLimbs)
        {
            // Reset all limbs
            foreach (Limb limb in limbs)
            {
                limb.Add();
            }
        }
    }

    public Mask.MaskTeam GetMaskTeam()
    {
        if (IsMasked())
        {
            return masks[0].maskTeam;
        }

        return Mask.MaskTeam.Neutral;
    }
}