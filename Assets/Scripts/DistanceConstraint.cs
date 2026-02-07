using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DistanceConstraint : MonoBehaviour
{
    public Transform target;
    public Vector2 offset;

    [Space]
    
    public float distance;
    public bool forceDistance;

    [Space]

    public float angleOffset;
    public float maxAngle;

    [Space]

    public bool selfUpdate;

    [Space]

    public DistanceConstraint childConstraint;

    private Quaternion initialRotation;
    private Vector2 worldPosition;

    private void Start()
    {
        initialRotation = transform.rotation;
        worldPosition = transform.position;

        if (childConstraint) childConstraint.selfUpdate = false;

        Update();
    }

    private void Update()
    {
        if (selfUpdate) Apply();
    }

    public void Apply()
    {
        if (target == null) return;

        Vector2 targetPosition = GetTargetPosition();

        Vector2 delta = worldPosition - targetPosition;

        float currentDistance = delta.magnitude;
        if (currentDistance == distance) return;

        if (currentDistance > distance || forceDistance)
        {
            worldPosition = targetPosition + distance * delta.normalized;
        }

        if (maxAngle > 0.0f)
        {
            float angle = Vector2.SignedAngle(-target.up, delta.normalized);
            if (Mathf.Abs(angle) > maxAngle)
            {
                float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);
                worldPosition = targetPosition + distance * Generic.RotateVector2(-target.up, Mathf.Deg2Rad * clampedAngle);
            }
        }

        transform.position = worldPosition;
        transform.rotation = initialRotation * Quaternion.FromToRotation(Vector3.up, -delta.normalized) * Quaternion.Euler(angleOffset * Vector3.forward);

        if (childConstraint) childConstraint.Apply();
    }

    private Vector2 GetTargetPosition()
    {
        if (!target) return (Vector2)transform.position;

        return target.position + target.TransformVector(offset);
    }
}
