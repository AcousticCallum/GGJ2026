using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed;
    public bool instant;

    public float mouseWeight;

    private void Update()
    {
        // Get player mask and return if not found
        PlayerMask playerMask = PlayerMask.instance;
        if (!playerMask) return;

        // Calculate target position
        Vector2 targetPosition = playerMask.transform.position;

        // Apply mouse weight
        if (mouseWeight != 0.0f)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(playerMask.GetMousePosition());
            targetPosition = Vector2.LerpUnclamped(targetPosition, mousePosition, mouseWeight);
        }

        // Move camera towards target position
        if (instant)
        {
            // Instantly move to the target position
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
        else
        {
            // Smoothly interpolate to the target position
            Vector2 newPosition = Vector2.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }
}
