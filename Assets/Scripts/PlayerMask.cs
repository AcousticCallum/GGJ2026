using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMask : Mask
{
    public static PlayerMask instance;

    [Space]

    public int prefabIndex;

    public bool canAim;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 mousePositionInput;
    private bool usingMouse;

    private static bool switching;

    protected override void Start()
    {
        base.Start();

        // Do nothing if not the controller
        if (!controller)
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
            return;
        }

        // Set instance
        if (PlayerMask.instance != this)
        {
            PlayerMask.instance = this;

            // Refresh PlayerInput
            PlayerInput playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
            playerInput.enabled = true;
        }
    }

    protected override void Update()
    {
        // Do nothing if not the controller
        if (!controller)
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
            return;
        }

        // Refresh instance
        if (PlayerMask.instance != this)
        {
            PlayerMask.instance = this;

            // Refresh PlayerInput
            PlayerInput playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

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
        if (body.IsMasked() || switching)
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
        if (ctx.started)
        {
            body.PrimaryAction();
        }

        if (ctx.canceled)
        {
            body.PrimaryActionEnd();
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            body.SecondaryAction();
        }

        if (ctx.canceled)
        {
            body.SecondaryActionEnd();
        }
    }

    public void OnSwitch(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            StartSwitch();
            return;
        }

        if (ctx.canceled)
        {
            FinishSwitch();
            return;
        }
    }

    private void StartSwitch()
    {
        switching = true;

        Time.timeScale = 0.2f;
    }

    private void FinishSwitch()
    {
        float bestDistance = float.MaxValue;
        Body bestBody = null;
        Body.allBodies.RemoveAll(item => item == null);
        foreach (Body checkBody in Body.allBodies)
        {
            if (!checkBody || !checkBody.rb || checkBody.IsMasked() || !checkBody.switchable) continue;

            if (usingMouse)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePositionInput);
                float distance = Vector2.Distance(checkBody.rb.position, mouseWorldPos);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestBody = checkBody;
            }
            else
            {
                Vector2 toCheckBody = (checkBody.rb.position - body.rb.position).normalized;
                float distance = Vector2.Angle(lookInput, toCheckBody);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestBody = checkBody;
            }
        }

        if (bestBody != null)
        {
            // Switch to bestBody
            body.switchable = false;
            bestBody.AddMask(Prefabs.instance.maskPrefabs[prefabIndex]);
            body.RemoveMask(this, true);

            // Refresh PlayerInput
            PlayerInput playerInput = bestBody.masks[0].GetComponent<PlayerInput>();
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

        Time.timeScale = 1.0f;

        switching = false;
    }

    public Vector2 GetMousePosition()
    {
        return mousePositionInput;
    }

    public Vector2 GetLook()
    {
        return lookInput;
    }

    public bool IsUsingMouse()
    {
        return usingMouse;
    }
}
