using UnityEngine;

public class EnemyMask : Mask
{
    protected override void Update()
    {
        // Do nothing if not the controller
        if (!controller) return;

        // Get direction from to player
        Vector2 playerPosition = PlayerMask.instance.body.rb.position;
        Vector2 direction = (playerPosition - body.rb.position).normalized;

        // Move and rotate the body towards the player
        body.Move(direction);
        body.Rotate(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f);
    }
}
