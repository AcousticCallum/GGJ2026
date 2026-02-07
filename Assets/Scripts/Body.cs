using UnityEngine;

using System.Collections.Generic;

public class Body : MonoBehaviour
{
    public static List<Body> allBodies = new List<Body>();

    [HideInInspector] public Rigidbody2D rb;

    public bool setCentreOfMass;
    public Vector2 centreOfMass;

    [Space]

    public Transform[] maskSlots;
    public List<Mask> masks;
    public Mask addMaskPrefabOnStart;

    [Space]

    public Limb[] limbs;

    [Space]

    public float moveSpeed;
    public float moveAcceleration;
    public float rotateSpeed;

    private Vector2 targetVelocity;
    private float targetRotation;

    public float knockbackResistance;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float rotation;

    public float spacingRadius;

    [Space]

    public float playerMaskDuration;

    [Space]

    public float deathDelay;

    public GameObject deathIndicator;

    private bool dying;
    private float deathTimer;

    public bool switchable = true;

    // Stat bonuses
    private float moveSpeedMultiplier = 1.0f;
    private float moveAccelerationMultiplier = 1.0f;
    private float rotateSpeedMultiplier = 1.0f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (setCentreOfMass) rb.centerOfMass = centreOfMass;

        foreach (Limb limb in limbs)
        {
            limb.body = this;
        }

        if (addMaskPrefabOnStart)
        {
            AddMask(addMaskPrefabOnStart);
        }

        Body.allBodies.Add(this);
    }

    protected virtual void Update()
    {
        // Initialise stat bonuses
        moveSpeedMultiplier = 1.0f;
        moveAccelerationMultiplier = 1.0f;
        rotateSpeedMultiplier = 1.0f;

        // Apply stat bonuses from mask (only once)
        if(IsMasked())
        {
            foreach (StatBonus statBonus in masks[0].statBonuses)
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
                Die();
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        // Apply movement and rotation to Rigidbody2D
        rb.MovePosition(rb.position + Time.fixedDeltaTime * velocity);
        rb.MoveRotation(rotation);
    }

    public void Move(Vector2 movement)
    {
        if (movement.magnitude > 1.0f) movement.Normalize();
        targetVelocity = moveSpeed * movement;
    }

    public void Rotate(float angle)
    {
        targetRotation = angle;
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

    public void PrimaryActionEnd()
    {
        foreach (Limb limb in limbs)
        {
            // Skip destroyed limbs
            if (limb.destroyed) continue;

            limb.PrimaryActionEnd();
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

    public void SecondaryActionEnd()
    {
        foreach (Limb limb in limbs)
        {
            // Skip destroyed limbs
            if (limb.destroyed) continue;

            limb.SecondaryActionEnd();
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
            RemoveMask(masks[0], true);
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

    public virtual void RemoveMask(Mask mask, bool removeAll = false)
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

        // Remove all masks if specified
        if (removeAll)
        {
            RemoveMask(masks[0], true);
        }
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

        if (switchable)
        {
            deathIndicator.SetActive(true);

            // Increment kill count for player mask
            PlayerMask.kills++;
        }
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

        if (switchable) deathIndicator.SetActive(false);
    }

    public void Die()
    {
        Body.allBodies.Remove(this);

        Destroy(gameObject);
    }

    public float GetAngularVelocity()
    {
        float nextRotation = Mathf.LerpAngle(rotation, targetRotation, rotateSpeedMultiplier * rotateSpeed * Time.deltaTime);
        return (nextRotation - rotation) / Time.deltaTime;
    }

    public Mask.MaskTeam GetMaskTeam()
    {
        if (IsMasked())
        {
            return masks[0].maskTeam;
        }

        return Mask.MaskTeam.Neutral;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spacingRadius);
    }
}