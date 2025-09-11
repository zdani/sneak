using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float mouseYawSensitivity = 360f; // deg/sec per mouse X unit

    [Header("Jump/Gravity")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpCooldown = 0.15f;

    [Header("Dodge (Step)")]
    [SerializeField] private float dodgeDistance = 4.0f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float minDodgeInput = 0.2f; // min move input magnitude to allow dodge

    [Header("Crouch")]
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

    [Header("Input Actions (New Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction; // hold
    [SerializeField] private InputActionReference dodgeAction;  // tap
    [SerializeField] private InputActionReference crouchAction; // toggle

    // Runtime default actions (used if references are unassigned)
    private InputAction? rtMove;
    private InputAction? rtLook;
    private InputAction? rtJump;
    private InputAction? rtSprint;
    private InputAction? rtDodge;
    private InputAction? rtCrouch;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultControllerHeight = controller.height;
        defaultControllerCenter = controller.center;

        // Ensure CharacterController is configured reasonably and not intersecting the ground
        SanitizeCharacterController();
    }

    void OnEnable()
    {
        EnsureRuntimeDefaults();
        EnableActions(moveAction, lookAction, jumpAction, sprintAction, dodgeAction, crouchAction);
        EnableAction(rtMove);
        EnableAction(rtLook);
        EnableAction(rtJump);
        EnableAction(rtSprint);
        EnableAction(rtDodge);
        EnableAction(rtCrouch);
    }

    void OnDisable()
    {
        DisableActions(moveAction, lookAction, jumpAction, sprintAction, dodgeAction, crouchAction);
        DisableAction(rtMove);
        DisableAction(rtLook);
        DisableAction(rtJump);
        DisableAction(rtSprint);
        DisableAction(rtDodge);
        DisableAction(rtCrouch);
    }

    void Update()
    {
        // Read input from the new Input System
        var moveAct = (moveAction != null && moveAction.action != null) ? moveAction.action : rtMove;
        Vector2 inputMove = (moveAct != null) ? moveAct.ReadValue<Vector2>() : Vector2.zero;
        inputMove = Vector2.ClampMagnitude(inputMove, 1f);
        var lookAct = (lookAction != null && lookAction.action != null) ? lookAction.action : rtLook;
        Vector2 look = (lookAct != null) ? lookAct.ReadValue<Vector2>() : Vector2.zero;
        float mouseX = look.x;
        var jAct = (jumpAction != null && jumpAction.action != null) ? jumpAction.action : rtJump;
        var sAct = (sprintAction != null && sprintAction.action != null) ? sprintAction.action : rtSprint;
        var dAct = (dodgeAction != null && dodgeAction.action != null) ? dodgeAction.action : rtDodge;
        var cAct = (crouchAction != null && crouchAction.action != null) ? crouchAction.action : rtCrouch;
        bool jumpPressed = jAct != null && jAct.WasPressedThisFrame();
        bool sprintHeldLocal = sAct != null && sAct.IsPressed();
        bool dodgeTap = dAct != null && dAct.WasPressedThisFrame();
        bool crouchToggle = cAct != null && cAct.WasPressedThisFrame();

        HandleCrouch(crouchToggle);
        if (controller.isGrounded) lastGroundedTime = Time.time;
        HandleJump(jumpPressed);
        HandleDodge(dodgeTap, inputMove);
        
        // Rotate character ONLY from mouse X input (no rotation from WASD)
        if (Mathf.Abs(mouseX) > 0.0001f)
        {
            float yawDelta = mouseX * mouseYawSensitivity * Time.deltaTime;
            transform.Rotate(0f, yawDelta, 0f, Space.World);
        }

        // Determine movement speeds
        bool canSprint = !isCrouched && !isDodging;
        float targetSpeed = isCrouched ? crouchSpeed : (sprintHeldLocal && canSprint ? runSpeed : walkSpeed);

        // Compute camera-relative desired direction
        Vector3 desiredDirWorld = GetCameraRelativeDirection(inputMove);
        Vector3 desiredPlanarVelocity = desiredDirWorld * targetSpeed;

        if (!isDodging)
        {
            ApplyAcceleration(desiredPlanarVelocity);
        }

        // If dodging, override planar velocity with dash
        Vector3 finalPlanarVelocity = isDodging ? (dodgeDirectionWorld * (dodgeDistance / Mathf.Max(0.01f, dodgeDuration))) : currentVelocityWorld;

        // No rotation from movement input; facing is controlled solely by mouse

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

    private void EnableAction(InputActionReference? reference)
    {
        if (reference == null || reference.action == null) return;
        if (!reference.action.enabled) reference.action.Enable();
    }

    private void DisableAction(InputActionReference? reference)
    {
        if (reference == null || reference.action == null) return;
        if (reference.action.enabled) reference.action.Disable();
    }

    private void EnableActions(params InputActionReference?[] refs)
    {
        foreach (var r in refs) EnableAction(r);
    }

    private void DisableActions(params InputActionReference?[] refs)
    {
        foreach (var r in refs) DisableAction(r);
    }

    private void EnableAction(InputAction? action)
    {
        if (action == null) return;
        if (!action.enabled) action.Enable();
    }

    private void DisableAction(InputAction? action)
    {
        if (action == null) return;
        if (action.enabled) action.Disable();
    }

    private void EnsureRuntimeDefaults()
    {
        if (rtMove == null)
        {
            rtMove = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Vector2");
            var comp = rtMove.AddCompositeBinding("2DVector");
            comp.With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            rtMove.AddBinding("<Gamepad>/leftStick");
        }
        if (rtLook == null)
        {
            rtLook = new InputAction(name: "Look", type: InputActionType.Value, expectedControlType: "Vector2");
            rtLook.AddBinding("<Mouse>/delta");
            rtLook.AddBinding("<Gamepad>/rightStick");
        }
        if (rtJump == null)
        {
            rtJump = new InputAction(name: "Jump", type: InputActionType.Button);
            rtJump.AddBinding("<Keyboard>/space");
            rtJump.AddBinding("<Gamepad>/buttonSouth");
        }
        if (rtSprint == null)
        {
            rtSprint = new InputAction(name: "Sprint", type: InputActionType.Button);
            rtSprint.AddBinding("<Keyboard>/leftShift");
            rtSprint.AddBinding("<Gamepad>/leftStickPress");
        }
        if (rtDodge == null)
        {
            rtDodge = new InputAction(name: "Dodge", type: InputActionType.Button);
            rtDodge.AddBinding("<Keyboard>/leftShift");
            rtDodge.AddBinding("<Gamepad>/rightShoulder");
        }
        if (rtCrouch == null)
        {
            rtCrouch = new InputAction(name: "Crouch", type: InputActionType.Button);
            rtCrouch.AddBinding("<Keyboard>/leftCtrl");
            rtCrouch.AddBinding("<Gamepad>/rightStickPress");
        }
    }

    private void SanitizeCharacterController()
    {
        // Remove physics components to avoid conflicts with CharacterController
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            Destroy(rb);
        }
        if (TryGetComponent<CapsuleCollider>(out var cap))
        {
            Destroy(cap);
        }

        // Sensible defaults if left at zeros
        if (controller.height < 1.2f) controller.height = 1.8f;
        float half = controller.height * 0.5f;
        if (controller.radius < 0.2f) controller.radius = Mathf.Min(0.5f, half - 0.05f);
        if (controller.skinWidth < 0.02f) controller.skinWidth = 0.08f;
        if (controller.stepOffset < 0.1f) controller.stepOffset = Mathf.Clamp(controller.height * 0.2f, 0.25f, 0.5f);
        if (controller.slopeLimit < 30f) controller.slopeLimit = 45f;
        controller.center = new Vector3(0f, half, 0f);

        // If starting intersecting the ground, nudge upward by a small amount
        if (Physics.CheckCapsule(transform.position + Vector3.up * (controller.radius + 0.01f),
                                  transform.position + Vector3.up * (controller.height - controller.radius - 0.01f),
                                  controller.radius, ~0, QueryTriggerInteraction.Ignore))
        {
            transform.position += Vector3.up * 0.2f;
        }
    }
}

