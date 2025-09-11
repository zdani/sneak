using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThiefController : MonoBehaviour
{
#nullable disable
    private CharacterController controller;
#nullable enable

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.6f;
    [SerializeField] private float runSpeed = 6.8f;
    [SerializeField] private float crouchSpeed = 2.0f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 24f;
    [SerializeField] private float rotationSpeed = 720f; // deg/sec

    [Header("Jump/Gravity")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpCooldown = 0.15f;

    [Header("Dodge (Step)")]
    [SerializeField] private KeyCode sprintDodgeKey = KeyCode.LeftShift; // tap to dodge, hold to sprint
    [SerializeField] private float dodgeDistance = 4.0f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float minDodgeInput = 0.2f; // min move input magnitude to allow dodge

    [Header("Crouch")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private float crouchHeightMultiplier = 0.6f;

    // State
    private Vector3 currentVelocityWorld; // includes only planar velocity in xz; y handled separately
    private float verticalVelocity;
    private bool isCrouched;
    private bool isDodging;
    private float dodgeTimeRemaining;
    private Vector3 dodgeDirectionWorld;
    private float lastDodgeTime;
    private float lastGroundedTime;
    private float lastJumpTime;
    private float defaultControllerHeight;
    private Vector3 defaultControllerCenter;

    // External input API (optional)
    private Vector2 fedMove;
    private bool fedJump;
    private bool fedCrouchToggle;
    private bool fedSprintHeld;
    private bool fedDodgeTap;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultControllerHeight = controller.height;
        defaultControllerCenter = controller.center;
    }

    void Update()
    {
        // Read input (keyboard/mouse by default, can be overridden by feed API)
        Vector2 inputMove = fedMove;
        inputMove.x += SafeAxis("Horizontal");
        inputMove.y += SafeAxis("Vertical");
        inputMove = Vector2.ClampMagnitude(inputMove, 1f);

        bool jumpPressed = fedJump || Input.GetKeyDown(KeyCode.Space);
        bool sprintHeld = fedSprintHeld || Input.GetKey(sprintDodgeKey);
        bool dodgeTap = fedDodgeTap || Input.GetKeyDown(sprintDodgeKey);
        bool crouchToggle = fedCrouchToggle || Input.GetKeyDown(crouchKey);

        fedMove = Vector2.zero;
        fedJump = false;
        fedCrouchToggle = false;
        fedSprintHeld = false;
        fedDodgeTap = false;

        HandleCrouch(crouchToggle);
        HandleGroundAndGravity();
        HandleJump(jumpPressed);
        HandleDodge(dodgeTap, inputMove);

        // Determine movement speeds
        bool canSprint = !isCrouched && !isDodging;
        float targetSpeed = isCrouched ? crouchSpeed : (sprintHeld && canSprint ? runSpeed : walkSpeed);

        // Compute camera-relative desired direction
        Vector3 desiredDirWorld = GetCameraRelativeDirection(inputMove);
        Vector3 desiredPlanarVelocity = desiredDirWorld * targetSpeed;

        if (!isDodging)
        {
            ApplyAcceleration(desiredPlanarVelocity);
        }

        // If dodging, override planar velocity with dash
        Vector3 finalPlanarVelocity = isDodging ? (dodgeDirectionWorld * (dodgeDistance / Mathf.Max(0.01f, dodgeDuration))) : currentVelocityWorld;

        // Face move direction if any (prioritize dodge dir)
        Vector3 lookDir = isDodging ? dodgeDirectionWorld : desiredDirWorld;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Compose motion
        Vector3 motion = finalPlanarVelocity;
        verticalVelocity += gravity * Time.deltaTime;
        if (controller.isGrounded && verticalVelocity < -2f) verticalVelocity = -2f;
        motion.y = verticalVelocity;

        // Move character
        controller.Move(motion * Time.deltaTime);

        // Update dodge timer
        if (isDodging)
        {
            dodgeTimeRemaining -= Time.deltaTime;
            if (dodgeTimeRemaining <= 0f)
            {
                isDodging = false;
                lastDodgeTime = Time.time;
            }
        }
    }

    private void HandleCrouch(bool toggle)
    {
        if (!toggle) return;
        isCrouched = !isCrouched;
        float targetHeight = isCrouched ? defaultControllerHeight * Mathf.Clamp(crouchHeightMultiplier, 0.3f, 1f) : defaultControllerHeight;
        Vector3 targetCenter = defaultControllerCenter;
        targetCenter.y = isCrouched ? defaultControllerCenter.y * crouchHeightMultiplier : defaultControllerCenter.y;
        controller.height = targetHeight;
        controller.center = targetCenter;
    }

    private void HandleGroundAndGravity()
    {
        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void HandleJump(bool pressed)
    {
        if (!pressed) return;
        if (isDodging) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;
        if (controller.isGrounded || Time.time - lastGroundedTime <= coyoteTime)
        {
            lastJumpTime = Time.time;
            verticalVelocity = Mathf.Sqrt(Mathf.Abs(2f * gravity * jumpHeight));
        }
    }

    private void HandleDodge(bool pressed, Vector2 inputMove)
    {
        if (!pressed) return;
        if (isDodging) return;
        if (Time.time - lastDodgeTime < dodgeCooldown) return;
        if (inputMove.magnitude < minDodgeInput) return;

        Vector3 dir = GetCameraRelativeDirection(inputMove);
        if (dir.sqrMagnitude < 0.0001f) return;

        isDodging = true;
        dodgeTimeRemaining = dodgeDuration;
        dodgeDirectionWorld = dir.normalized;
        // small downward push so we remain grounded at step start
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
    }

    private void ApplyAcceleration(Vector3 desiredPlanarVelocity)
    {
        Vector3 currentPlanar = new Vector3(currentVelocityWorld.x, 0f, currentVelocityWorld.z);
        Vector3 delta = desiredPlanarVelocity - currentPlanar;
        float accel = (desiredPlanarVelocity.magnitude > currentPlanar.magnitude) ? acceleration : deceleration;
        Vector3 change = Vector3.ClampMagnitude(delta, accel * Time.deltaTime);
        Vector3 newPlanar = currentPlanar + change;
        currentVelocityWorld = new Vector3(newPlanar.x, 0f, newPlanar.z);
    }

    private Vector3 GetCameraRelativeDirection(Vector2 inputMove)
    {
        Transform camT = (UnityEngine.Camera.main != null) ? UnityEngine.Camera.main.transform : transform;
        Vector3 forward = camT.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = camT.right; right.y = 0f; right.Normalize();
        Vector3 dir = (forward * inputMove.y) + (right * inputMove.x);
        return dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }

    private static float SafeAxis(string axis)
    {
        try { return Input.GetAxisRaw(axis); } catch { return 0f; }
    }

    // Optional feed-input API for new Input System
    public void FeedMoveInput(Vector2 move) { fedMove += move; }
    public void FeedJumpPressed() { fedJump = true; }
    public void FeedSprintHeld(bool held) { fedSprintHeld = fedSprintHeld || held; }
    public void FeedDodgeTap() { fedDodgeTap = true; }
    public void FeedCrouchToggle() { fedCrouchToggle = true; }
}

