using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
#nullable disable
    private Camera engineCamera;
    [SerializeField] private Transform target; // Player character to follow
#nullable enable
    [Header("Target")]

    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f); // eye height

    [Header("Orbit & Distance")]
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float minDistance = 1.2f;
    [SerializeField] private float maxDistance = 6.0f;
    [SerializeField] private float shoulderOffset = 0.45f; // horizontal shoulder offset
    [SerializeField] private bool startLeftShoulder = false;

    [Header("Rotation")]
    [SerializeField] private float yawSensitivity = 180f;   // deg/sec per input unit
    [SerializeField] private float pitchSensitivity = 135f; // deg/sec per input unit
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float rotationSmoothTime = 0.05f; // smaller = snappier

    [Header("Follow Smoothing")]
    [SerializeField] private float followSmoothTime = 0.06f; // lerp to desired pivot

    [Header("Collision")] 
    [SerializeField] private LayerMask collisionMask = ~0; // default: collide with everything
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionBuffer = 0.1f;
    [SerializeField] private float distanceAdjustSpeed = 12f;

    

    [Header("Recentering & Vertical Compensation")]
    [SerializeField] private float recenterWaitTime = 1.0f; // seconds w/o input
    [SerializeField] private float recenterYawSpeed = 220f; // deg/sec when auto recentering
    [SerializeField] private float verticalCompensation = 0.5f; // adds pitch based on vertical velocity

    [Header("FOV & Velocity Effects")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovLerpSpeed = 6f;
    [SerializeField] private float sprintSpeedThreshold = 6.5f; // world units/s

    // Runtime state
    private float yaw;
    private float pitch;
    private float yawVelocity;
    private float pitchVelocity;
    private float currentDistance;
    private float desiredDistance;
    private bool leftShoulder;
    private float lastInputTime;
    private Vector3 previousTargetPosition;

    [Header("Input Actions (New Input System)")]
    [SerializeField] private InputActionReference lookAction;   // Vector2
    [SerializeField] private InputActionReference zoomAction;   // float (scroll or gamepad)
    [SerializeField] private InputActionReference shoulderSwapAction; // button

    private bool shoulderSwapPressed;

    void Awake()
    {
        engineCamera = GetComponent<Camera>();
        leftShoulder = startLeftShoulder;
        desiredDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        currentDistance = desiredDistance;
        EnableAction(lookAction);
        EnableAction(zoomAction);
        EnableAction(shoulderSwapAction);
    }

    void Start()
    {
        var euler = transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = NormalizePitch(euler.x);
        previousTargetPosition = target.position;
        if (engineCamera != null)
        {
            engineCamera.fieldOfView = baseFOV;
        }
    }

    void OnDisable()
    {
        DisableAction(lookAction);
        DisableAction(zoomAction);
        DisableAction(shoulderSwapAction);
    }

    void Update()
    {
        // Gather inputs from the new Input System only
        Vector2 lookInput = (lookAction != null && lookAction.action != null) ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;
        float zoomInput = (zoomAction != null && zoomAction.action != null) ? zoomAction.action.ReadValue<float>() : 0f;
        HandleInput(lookInput, zoomInput, ReadShoulderSwap());

        // Reset one-shot
        shoulderSwapPressed = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + targetOffset;
        Vector3 targetVelocity = (target.position - previousTargetPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousTargetPosition = target.position;

        // Aim orientation is player-controlled only (no lock-on)

        // Apply vertical compensation inspired by Sekiro: pitch nudges with vertical motion
        float verticalPitchAdjust = -targetVelocity.y * verticalCompensation;
        pitch = Mathf.Clamp(pitch + verticalPitchAdjust * Time.deltaTime, minPitch, maxPitch);

        // Compute camera pivot and shoulder offset
        Quaternion yawOnly = Quaternion.Euler(0f, yaw, 0f);
        Vector3 shoulder = yawOnly * (leftShoulder ? Vector3.left : Vector3.right) * shoulderOffset;
        Vector3 pivot = targetPos + shoulder;

        // Desired rotation
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Distance with collision handling
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        currentDistance = Mathf.MoveTowards(currentDistance, desiredDistance, distanceAdjustSpeed * Time.deltaTime);
        float adjustedDistance = ResolveCollision(pivot, desiredRotation, currentDistance);

        // Final position
        Vector3 desiredCamPos = pivot - (desiredRotation * Vector3.forward) * adjustedDistance;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredCamPos, Mathf.Clamp01(Time.deltaTime / Mathf.Max(0.0001f, followSmoothTime)));
        transform.position = smoothedPos;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Mathf.Clamp01(Time.deltaTime / Mathf.Max(0.0001f, rotationSmoothTime)));

        // Recenter if needed (only when moving forward relative to camera)
        Vector3 planarVel = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
        if (Vector3.Dot(planarVel, transform.forward) > 0.2f)
        {
            AutoRecenterYaw(targetVelocity);
        }

        // FOV kick
        if (engineCamera != null)
        {
            float speed = new Vector3(targetVelocity.x, 0f, targetVelocity.z).magnitude;
            float targetFOV = Mathf.Lerp(baseFOV, sprintFOV, Mathf.InverseLerp(sprintSpeedThreshold * 0.6f, sprintSpeedThreshold, speed));
            engineCamera.fieldOfView = Mathf.Lerp(engineCamera.fieldOfView, targetFOV, 1f - Mathf.Exp(-fovLerpSpeed * Time.deltaTime));
        }
    }

    private void HandleInput(Vector2 lookInput, float zoomInput, bool shoulderSwap)
    {
        float dt = Time.deltaTime;

        // Look input
        if (lookInput.sqrMagnitude > 0.0001f)
        {
            lastInputTime = Time.time;
            float x = lookInput.x;
            float y = lookInput.y * (invertY ? 1f : -1f);

            float targetYaw = yaw + x * yawSensitivity * dt;
            float targetPitch = Mathf.Clamp(pitch + y * pitchSensitivity * dt, minPitch, maxPitch);
            yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
            pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);
        }

        // Zoom (scroll wheel)
        if (Mathf.Abs(zoomInput) > 0.0001f)
        {
            desiredDistance = Mathf.Clamp(desiredDistance + (-zoomInput) * 1.0f, minDistance, maxDistance);
        }

        // Shoulder swap
        if (shoulderSwap)
        {
            leftShoulder = !leftShoulder;
        }

        // No lock-on in this game; camera is fully manual with recentering
    }

    
    private void EnableAction(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return;
        if (!reference.action.enabled) reference.action.Enable();
    }

    private void DisableAction(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return;
        if (reference.action.enabled) reference.action.Disable();
    }

    private float ResolveCollision(Vector3 pivot, Quaternion rotation, float desiredDist)
    {
        Vector3 dir = rotation * Vector3.back; // back from pivot to camera
        Vector3 desiredPos = pivot + dir * desiredDist;

        // Sphere cast from pivot towards desired position
        Vector3 castDir = (desiredPos - pivot).normalized;
        float castDist = desiredDist;
        if (Physics.SphereCast(pivot, collisionRadius, castDir, out RaycastHit hit, castDist + collisionBuffer, collisionMask, QueryTriggerInteraction.Ignore))
        {
            float hitDist = Mathf.Max(minDistance, hit.distance - collisionBuffer);
            return Mathf.Min(desiredDist, hitDist);
        }
        return desiredDist;
    }

    private void AutoRecenterYaw(Vector3 targetVelocity)
    {
        if (Time.time - lastInputTime < recenterWaitTime) return;
        Vector3 planarVel = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
        if (planarVel.sqrMagnitude < 0.01f) return;
        // Recenter yaw behind the target's moving direction
        float targetYaw = Mathf.Atan2(planarVel.x, planarVel.z) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(yaw, targetYaw);
        float step = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), recenterYawSpeed * Time.deltaTime);
        yaw += step;
    }

    private static float NormalizePitch(float xAngle)
    {
        float a = xAngle;
        if (a > 180f) a -= 360f;
        return a;
    }

    

    

    private bool ReadShoulderSwap()
    {
        if (shoulderSwapAction == null || shoulderSwapAction.action == null) return false;
        if (!shoulderSwapPressed && shoulderSwapAction.action.WasPressedThisFrame())
        {
            shoulderSwapPressed = true;
            return true;
        }
        return false;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
    
}
