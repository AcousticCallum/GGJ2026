using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMask : Mask
{
    [Space]

    public bool canAim;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 mousePositionInput;
    private bool usingMouse;

    protected override void Update()
    {
        // Do nothing if not the controller
        if (!controller) return;

        // Move and rotate the body based on input
        body.Move(moveInput);

        if (canAim)
        {
            if (usingMouse)
            {
                // Rotate towards mouse position
                Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePositionInput);
                Vector2 direction = (worldMousePosition - body.rb.position).normalized;
                body.Rotate(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f);
            }
            else
            {
                // Rotate towards lookInput
                if (lookInput != Vector2.zero) body.Rotate(Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg - 90.0f);
            }
        }
        else
        {
            // Rotate towards moveInput
            if (moveInput != Vector2.zero) body.Rotate(Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90.0f);
        }
    }

    public override void OnRemove()
    {
        // Call base.OnRemove() if this isn't the last mask
        if (body.IsMasked())
        {
            base.OnRemove();

            return;
        }

        // Reload the scene if this is the last mask
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    // Gamepad only
    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
        if (lookInput != Vector2.zero) usingMouse = false;
    }

    // Mouse only
    public void OnMousePosition(InputAction.CallbackContext ctx)
    {
        mousePositionInput = ctx.ReadValue<Vector2>();
        usingMouse = true;
    }

    public void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            body.PrimaryAction();
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            body.SecondaryAction();
        }
    }
}
