using Unity.Mathematics;
using UnityEngine;

public class LegLimb : Limb
{
    [Space]

    public Transform footTarget;
    public Transform foot;

    public float stepSizeMin;
    public float stepSizeMax;
    public float stepSpeed;

    public float length;

    public float prediction;

    public LegLimb[] opposingLimbs;

    private Vector2 lastPos;
    private Vector2 currentPos;
    private Vector2 targetPos;

    private bool stepping;
    private float stepLerp;

    protected override void Start()
    {
        base.Start();

        currentPos = foot.position;
    }

    protected override void Update()
    {
        TryStartStepping();

        if (stepping)
        {
            stepLerp = Mathf.Min(stepLerp + stepSpeed * Time.deltaTime, 1.0f);

            currentPos = transform.TransformPoint(Vector2.Lerp(lastPos, targetPos, stepLerp));

            if (stepLerp == 1.0f)
            {
                stepping = false;

                currentPos = footTarget.position;
            }
        }

        foot.position = currentPos;
    }

    public void TryStartStepping()
    {
        if (stepping)
        {
            return;
        }

        float distance = Vector2.Distance(currentPos, footTarget.position);

        if (distance > stepSizeMin)
        {
            // If under the max step size, check if any opposing limbs are stepping
            if (distance < stepSizeMax)
            {
                foreach (LegLimb limb in opposingLimbs)
                {
                    if (limb.IsStepping())
                    {
                        return;
                    }
                }
            }

            lastPos = (Vector2)transform.InverseTransformPoint(foot.position);
            currentPos = foot.position;
            targetPos = (Vector2)transform.InverseTransformPoint((Vector2)footTarget.position + prediction * body.velocity);
            Debug.Log(prediction * body.velocity);

            // Distance constraint
            if (targetPos.magnitude > length) targetPos = length * targetPos.normalized;

            stepping = true;
            stepLerp = 0.0f;
        }
    }

    public bool IsStepping()
    {
        return stepping;
    }
}
