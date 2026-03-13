using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMask : Mask
{
    public static PlayerMask instance;

    [Space]

    private static bool switching;
    private static List<Body> switchableBodies = new();
    private static Body switchBody;

    public static int kills;
    public static int playerSouls;

    public int prefabIndex;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 mousePositionInput;
    private bool usingMouse;

    [Space]

    public bool bodyTimerActive;
    public float bodyTimer;
    public float bodyTimerSlowdownMultiplier;
    public float bodyTimerSlowdownDuration;

    [Space]
     
    public float maxSwitchDistance;

    [Space]

    public float maxInteractDistance;

    private static bool spawnedIn = false;

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

        // Spawn in.
        if (!PlayerMask.spawnedIn)
        {
            PlayerMask.spawnedIn = true;

            int shrineID = GameState.GetState("Shrine");

            if (shrineID == 0) return;

            foreach (Shrine shrine in Shrine.allShrines)
            {
                if (shrine.shrineID != shrineID) continue;

                Debug.Log("Spawning in at shrine " + shrineID);

                body.rb.simulated = false; // Disable physics while moving to prevent unwanted collisions

                // Set position and rotation to shrine spawn point.
                body.transform.position = (Vector3)shrine.GetSpawnPosition();
                body.transform.rotation = Quaternion.Euler(0.0f, 0.0f, shrine.GetSpawnRotation());

                body.rb.simulated = true; // Re-enable physics
                break;
            }
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

        // Handle body timer, negative pauses body timer
        if (bodyTimerActive && bodyTimer > 0.0f)
        {
            // Update body timer
            bodyTimer = Mathf.Max(bodyTimer - Time.deltaTime, 0.0f);

            // Apply time slowdown effect
            if (bodyTimer < bodyTimerSlowdownDuration)
            {
                float t = Mathf.InverseLerp(bodyTimerSlowdownDuration, 0.0f, bodyTimer);
                Time.timeScale = Mathf.Lerp(1.0f, bodyTimerSlowdownMultiplier, t);
            }
            else
            {
                Time.timeScale = 1.0f;
            }

            // Check for body timer expiration
            if (bodyTimer == 0.0f)
            {
                Die();
            }
        }

        // Update switch
        UpdateSwitch();
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

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Interact();
            return;
        }
    }

    private void StartSwitch()
    {
        switching = true;
    }

    private void UpdateSwitch()
    {
        // Make a copy of the current switchable bodies to check after
        List<Body> lastSwitchableBodies = new(switchableBodies);
        lastSwitchableBodies.RemoveAll(item => !item);

        // Refresh the list
        switchableBodies.Clear();

        // Check for switchable bodies and add them to the list
        Body.allBodies.RemoveAll(item => !item);
        foreach (Body checkBody in Body.allBodies)
        {
            if (!checkBody.AllowSwitch()) continue;

            // Check distance from mask to body doesn't exceed maxSwitchDistance.
            if (Vector2.Distance(transform.position, checkBody.rb.position) > maxSwitchDistance) continue;

            switchableBodies.Add(checkBody);
        }

        // Enable indicator on switchable bodies through updating switchBody.
        UpdateSwitchBody();

        // Disable indicator on bodies that are no longer switchable
        foreach (Body lastSwitchableBody in lastSwitchableBodies)
        {
            if (switchableBodies.Contains(lastSwitchableBody)) continue;

            lastSwitchableBody.switchIndicator.gameObject.SetActive(false);
        }
    }

    private void UpdateSwitchBody()
    {
        float bestDistance = float.MaxValue;
        Body bestBody = null;
        foreach (Body checkBody in switchableBodies)
        {
            // Enable switch indicator on switchable bodies
            checkBody.switchIndicator.gameObject.SetActive(true);

            if (usingMouse)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePositionInput);
                float distance = Vector2.Distance(checkBody.rb.position, mouseWorldPos);

                UpdateBodySwitchIndicator(checkBody.switchIndicator, distance);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestBody = checkBody;
            }
            else
            {
                Vector2 toCheckBody = (checkBody.rb.position - body.rb.position).normalized;
                float distance = Vector2.Angle(lookInput, toCheckBody);

                UpdateBodySwitchIndicator(checkBody.switchIndicator, distance);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestBody = checkBody;
            }
        }

        switchBody = bestBody;
    }

    private void UpdateBodySwitchIndicator(Rotator switchIndicator, float distance)
    {
        float t = Mathf.InverseLerp(3.0f, 0.5f, distance);
        t = t * t;

        float scale = Mathf.Lerp(1.0f, 2.0f, t);
        switchIndicator.transform.localScale = new Vector3(scale, scale, 1.0f);

        float multiplier = Mathf.Lerp(1.0f, 3.0f, t);
        switchIndicator.multiplier = multiplier;
    }

    private void FinishSwitch()
    {
        UpdateSwitch();

        if (switchBody)
        {
            // Switch to bestBody
            body.switchable = false;
            switchBody.AddMask(Prefabs.instance.maskPrefabs[prefabIndex]);
            body.RemoveMask(this, true);

            // Refresh body timer
            bodyTimer = switchBody.playerMaskDuration;

            // Refresh PlayerInput
            PlayerInput playerInput = switchBody.masks[0].GetComponent<PlayerInput>();
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

        switching = false;
    }

    private void Interact()
    {
        float bestDistance = float.MaxValue;
        Interactable bestInteractable = null;
        Interactable.allInteractables.RemoveAll(item => !item);
        foreach (Interactable checkInteractable in Interactable.allInteractables)
        {
            if (!checkInteractable) continue;

            // Check distance from mask to interactable doesn't exceed maxInteractDistance.
            if (Vector2.Distance(transform.position, checkInteractable.transform.position) > maxInteractDistance) continue;

            if (usingMouse)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePositionInput);
                float distance = Vector2.Distance((Vector2)checkInteractable.transform.position, mouseWorldPos);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestInteractable = checkInteractable;
            }
            else
            {
                Vector2 toCheckBody = ((Vector2)checkInteractable.transform.position - body.rb.position).normalized;
                float distance = Vector2.Angle(lookInput, toCheckBody);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                bestInteractable = checkInteractable;
            }
        }

        if (bestInteractable)
        {
            bestInteractable.Interact();
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

        Die();
    }

    private void Die()
    {
        PlayerMask.kills = 0;

        PlayerMask.spawnedIn = false;

        // Reload the scene if this is the last mask
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void AddSouls(int amount)
    {
        souls = PlayerMask.playerSouls;

        base.AddSouls(amount);

        PlayerMask.playerSouls = souls;
    }

    public override bool TryRemoveSouls(int amount, bool force = false, bool checkOnly = false)
    {
        souls = PlayerMask.playerSouls;

        bool result = base.TryRemoveSouls(amount, force, checkOnly);

        PlayerMask.playerSouls = souls;

        return result;
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
