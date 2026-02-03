using TMPro;
using UnityEngine;
using UnityEngine.U2D.IK;

public class ArmLimb : Limb
{
    public Transform handTarget;
    public Transform hand;

    public Vector2 targetOffset;

    public float speed;
    public float acceleration;
    public float length;
    public float minimumLength;
    public float stoppingDistance;
    private Vector2 velocity = Vector2.zero;

    public float knockbackRatio;
    public float knockbackResistance;

    public float swingRate;
    public float swingAngle;
    public float swingStartOffset;
    private float swingTimer;
    private bool swinging;

    public bool stunned;
    private float stunTimer;

    public bool flipOnOppositeSide;

    public ArmLimb[] opposingLimbs;

    public LimbSolver2D solver;

    protected override void Update()
    {
        stunTimer = Mathf.Max(0.0f, stunTimer - Time.deltaTime);
        if (stunTimer == 0.0f) stunned = false;

        if (!body.IsMasked()) return;

        if (stunned)
        {
            ResetHandTargetPosition();
        }
        {
            UpdateHandTargetPosition();
        }

        UpdateHandPosition();

        // Flip solver based on hand position
        if (flipOnOppositeSide)
        {
            Vector2 localHandPosition = transform.InverseTransformPoint(hand.position);
            if (localHandPosition.x != 0.0f) solver.flip = localHandPosition.x < 0.0f ? true : false;
        }
    }

    protected override void OnAdd()
    {
        swinging = false;
    }

    public void UpdateHandTargetPosition()
    {
        // Calculate target position
        Vector2 targetPosition = transform.position;
        bool applyOffset = true;
        if (GetMaskTeam() == Mask.MaskTeam.Friendly)
        {
            if (PlayerMask.instance.IsUsingMouse())
            {
                targetPosition = Camera.main.ScreenToWorldPoint(PlayerMask.instance.GetMousePosition());
            }
            else
            {
                Vector2 look = PlayerMask.instance.GetLook();
                targetPosition = (Vector2)transform.position + length * look;
            }
        }
        else if (GetMaskTeam() == Mask.MaskTeam.Hostile)
        {
            // Is there player to target
            if (PlayerMask.instance)
            {
                targetPosition = PlayerMask.instance.body.rb.position;
            }

            swinging = true; // Always swing for hostile arms
        }

        // Apply offset
        if (applyOffset) targetPosition += (Vector2)body.transform.TransformVector(targetOffset);

        // Apply swing
        if (swinging)
        {
            swingTimer += swingRate * Time.deltaTime;
            if (swingTimer > 1.0f) swingTimer -= 1.0f;

            Vector2 toTarget = (targetPosition - (Vector2)transform.position);
            targetPosition = (Vector2)transform.position + toTarget.magnitude * RotateVector2(toTarget.normalized, Mathf.Deg2Rad * GetSwingAngle());
        }
        else
        {
            swingTimer = 0.0f;
        }

        // Update hand target
        handTarget.position = targetPosition;
        handTarget.position = GetClampedPosition(handTarget.position);
    }

    public void UpdateHandPosition()
    {
        // Accelerate towards hand target
        Vector2 toHandTarget = handTarget.position - hand.position;
        if (toHandTarget.magnitude > stoppingDistance) toHandTarget.Normalize();
        else toHandTarget = Vector2.zero;
        velocity = Vector2.MoveTowards(velocity, toHandTarget * speed, acceleration * Time.deltaTime);
        //velocity = Vector2.ClampMagnitude(velocity, speed);

        // Move hand towards target
        hand.position += Time.deltaTime * new Vector3(velocity.x, velocity.y, 0.0f);
        hand.position = GetClampedPosition(hand.position);
    }

    public void ResetHandTargetPosition()
    {
        handTarget.position = hand.position;
    }

    public void GiveKnockback(Vector2 knockback)
    {
        knockback *= knockbackRatio;

        if (knockbackResistance < 0.0f)
        {
            knockback *= (1.0f - knockbackResistance);
        }
        else
        {
            knockback = Vector2.Lerp(knockback, velocity, knockbackResistance);
        }

        velocity = knockback;
    }

    public void Stun(float duration)
    {
        stunned = true;
        stunTimer = Mathf.Max(stunTimer, duration);
    }

    public override void PrimaryAction()
    {
        swinging = true;
    }

    public override void PrimaryActionEnd()
    {
        swinging = false;
    }

    private Vector2 GetClampedPosition(Vector2 pos)
    {
        float distance = Vector2.Distance(transform.position, pos);
        if (distance > length)
        {
            pos = (Vector2)transform.position + length * (pos - (Vector2)transform.position).normalized;
        }
        else if (distance < minimumLength)
        {
            pos = (Vector2)transform.position + minimumLength * (pos - (Vector2)transform.position).normalized;
        }

        return pos;
    }

    public float GetSwingAngle()
    {
        return Mathf.Cos((swingStartOffset + swingTimer) * 2.0f * Mathf.PI) * swingAngle;
    }

    public static Vector2 RotateVector2(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}
