using UnityEngine;
using UnityEngine.U2D.IK;

public class ArmLimb : Limb
{
    public Transform handTarget;
    public Transform hand;

    public Vector2 targetOffset;

    public float speed;
    public float length;

    public bool flipOnOppositeSide;

    public ArmLimb[] opposingLimbs;

    public LimbSolver2D solver;

    protected override void Update()
    {
        if (!body.IsMasked()) return;

        if (body.GetMaskTeam() == Mask.MaskTeam.Friendly)
        {
            if (PlayerMask.instance.IsUsingMouse())
            {
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(PlayerMask.instance.GetMousePosition());
                targetPosition += (Vector2)body.transform.TransformVector(targetOffset);

                float distance = Vector2.Distance(transform.position, targetPosition);
                if (distance > length)
                {
                    targetPosition = (Vector2)transform.position + length * (targetPosition - (Vector2)transform.position).normalized;
                }

                handTarget.position = targetPosition;
            }
            else
            {
                Vector2 look = PlayerMask.instance.GetLook();
                Vector2 targetPosition = (Vector2)transform.position + length * look;
                targetPosition += (Vector2)body.transform.TransformVector(targetOffset);

                handTarget.position = targetPosition;
            }
        }
        else if (body.GetMaskTeam() == Mask.MaskTeam.Hostile)
        {
            if (!PlayerMask.instance) return;

            Vector2 targetPosition = PlayerMask.instance.body.rb.position;
            targetPosition += (Vector2)body.transform.TransformVector(targetOffset);

            float distance = Vector2.Distance(transform.position, targetPosition);
            if (distance > length)
            {
                targetPosition = (Vector2)transform.position + length * (targetPosition - (Vector2)transform.position).normalized;
            }

            handTarget.position = targetPosition;
        }

        hand.position = Vector2.MoveTowards(hand.position, handTarget.position, speed * Time.deltaTime);

        if (flipOnOppositeSide)
        {
            Vector2 localHandPosition = transform.InverseTransformPoint(hand.position);
            if (localHandPosition.x != 0.0f) solver.flip = localHandPosition.x < 0.0f ? true : false;
        }
    }

    public override void PrimaryAction()
    {
        PlayerMask.instance.canAim = false;
    }

    public override void PrimaryActionEnd()
    {
        PlayerMask.instance.canAim = true;
    }
}
