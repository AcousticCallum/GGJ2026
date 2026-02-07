using UnityEngine;

public class EnemyMask : Mask
{
    public float moveCooldown;
    public float moveDuration;

    public bool canStopPrimaryAction;
    public bool canStopSecondaryAction;

    private float moveTimer;
    private bool moving;

    public LayerMask obstacleMask;
    public float detectionArcRadius;
    public float detectionArcAngle;
    public float detectionDuration;
    private float detectionTimer;

    protected override void Update()
    {
        // Do nothing if not the controller
        if (!controller) return;

        if(!PlayerMask.instance) return;

        // Get target position (player position)
        Vector2 targetPosition = PlayerMask.instance.body.rb.position;

        // Detection and detection timer
        if (ValidDetection(targetPosition))
        {
            detectionTimer = detectionDuration;
        }
        else
        {
            detectionTimer = Mathf.Max(0.0f, detectionTimer - Time.deltaTime);
        }

        // Do no movement or actions if not detected
        if (detectionTimer == 0.0f)
        {
            NoDetectionUpdate();
            return;
        }

        // Apply spacing from other bodies on the same team
        Body.allBodies.RemoveAll(item => item == null);
        foreach (Body checkBody in Body.allBodies)
        {
            if (checkBody == body) continue;

            // CheckBody is on player team
            if (checkBody.GetMaskTeam() == MaskTeam.Friendly) continue;

            if (Vector2.Distance(body.rb.position, checkBody.rb.position) < body.spacingRadius + checkBody.spacingRadius)
            {
                targetPosition = checkBody.rb.position + (body.spacingRadius + checkBody.spacingRadius) * (body.rb.position - checkBody.rb.position).normalized;
            }
        }

        Vector2 direction = (targetPosition - body.rb.position).normalized;

        // Handle movement timing
        moveTimer = Mathf.Max(0.0f, moveTimer - Time.deltaTime);
        if (moveTimer == 0.0f)
        {
            if (moving)
            {
                // Stop moving
                moving = false;
                moveTimer = moveCooldown;

                // End primary action when stopping move
                if (canStopPrimaryAction) body.PrimaryActionEnd();

                // End secondary action when stopping move
                if (canStopSecondaryAction) body.SecondaryActionEnd();
            }
            else
            {
                // Start moving
                moving = true;
                moveTimer = moveDuration;

                // Perform primary action when starting to move
                body.PrimaryAction();

                // Perform secondary action when starting to move
                body.SecondaryAction();
            }
        }

        if (moving)
        {
            // Move forward and rotate the body towards the player
            body.Move(body.transform.up);
            if (direction != Vector2.zero) body.Rotate(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f);
        }
        else
        {
            // Stop movement
            body.Move(Vector2.zero);
        }
    }

    public void NoDetectionUpdate()
    {
        // Do nothing if not the controller
        if (!controller) return;

        // Stop movement
        body.Move(Vector2.zero);
    }

    private bool ValidDetection(Vector2 targetPosition)
    {
        // Target is out of detection range
        if (Vector2.Distance(body.rb.position, targetPosition) > detectionArcRadius) return false;

        // Target is out of detection arc
        if (Vector2.Angle(body.transform.up, (targetPosition - (Vector2)body.transform.position).normalized) > detectionArcAngle / 2.0f) return false;

        // Target is behind an obstacle
        Vector2 toTarget = targetPosition - (Vector2)body.transform.position;
        if (Physics2D.Raycast(body.transform.position, toTarget.normalized, toTarget.magnitude, obstacleMask)) return false;

        // Target is valid
        return true;
    }
}
