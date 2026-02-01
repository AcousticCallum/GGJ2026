using UnityEngine;

public class EnemyMask : Mask
{
    public float moveCooldown;
    public float moveDuration;

    private float moveTimer;
    private bool moving;

    protected override void Update()
    {
        // Do nothing if not the controller
        if (!controller) return;

        if(!PlayerMask.instance) return;

        // Get direction from to player
        Vector2 playerPosition = PlayerMask.instance.body.rb.position;
        Vector2 direction = (playerPosition - body.rb.position).normalized;

        // Handle movement timing
        moveTimer = Mathf.Max(0.0f, moveTimer - Time.deltaTime);
        if (moveTimer == 0.0f)
        {
            if (moving)
            {
                // Stop moving
                moving = false;
                moveTimer = moveCooldown;
            }
            else
            {
                // Start moving
                moving = true;
                moveTimer = moveDuration;

                // Perform primary action when starting to move
                body.PrimaryAction();
                body.PrimaryActionEnd();
            }
        }

        if (moving)
        {
            // Move forward and rotate the body towards the player
            body.Move(body.transform.up);
            body.Rotate(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f);
        }
        else
        {
            // Stop movement
            body.Move(Vector2.zero);
        }
    }
}
