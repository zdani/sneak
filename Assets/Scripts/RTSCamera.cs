using UnityEngine;

public class RTSCamera : MonoBehaviour
{
	[Header("Follow")]
	[SerializeField] private Transform followTarget;
	[SerializeField, Range(0.01f, 1f)] private float followSmoothTime = 0.12f;

	[Header("Movement")]
	[SerializeField] private float basePanSpeed = 12f;
	[SerializeField] private bool edgePanEnabled = true;
	[SerializeField] private int edgeThickness = 8;

	[Header("Rotation & Zoom")]
	[SerializeField] private float yawDegrees = 0f;
	[SerializeField, Range(10f, 80f)] private float pitchDegrees = 55f;
	[SerializeField] private float rotateSpeed = 90f;
	[SerializeField] private float distance = 15f;
	[SerializeField] private float minDistance = 6f;
	[SerializeField] private float maxDistance = 40f;
	[SerializeField] private float zoomSpeed = 8f;

	private Vector3 focusPosition;
	private Vector3 followVelocity;
	private bool subscribed;

	void Awake()
	{
		focusPosition = transform.position;
		var e = transform.rotation.eulerAngles;
		yawDegrees = e.y;
	}

	void LateUpdate()
	{
		HandleRotation();
		HandleZoom();
		HandleMovement();
		UpdateTransform();
	}

	void OnEnable()
	{
		if (!subscribed)
		{
			EventManager.Instance.OnUnitSelected += HandleUnitSelected;
			subscribed = true;
		}
	}

	void OnDisable()
	{
		if (subscribed)
		{
			EventManager.Instance.OnUnitSelected -= HandleUnitSelected;
			subscribed = false;
		}
	}

	private void HandleMovement()
	{
		if (followTarget)
		{
			focusPosition = Vector3.SmoothDamp(focusPosition, followTarget.position, ref followVelocity, followSmoothTime);
			return;
		}

		Vector2 input = ReadMoveInput();
		if (edgePanEnabled)
		{
			input += ReadEdgePan();
		}

		if (input.sqrMagnitude > 1f) input.Normalize();

		// Pan speed scales slightly with zoom distance to feel consistent
		float panSpeed = basePanSpeed * Mathf.Lerp(0.75f, 1.75f, Mathf.InverseLerp(minDistance, maxDistance, distance));

		Quaternion yawRot = Quaternion.Euler(0f, yawDegrees, 0f);
		Vector3 right = yawRot * Vector3.right;
		Vector3 forward = yawRot * Vector3.forward;
		forward.y = 0f;
		right.y = 0f;

		Vector3 delta = (right * input.x + forward * input.y) * (panSpeed * Time.deltaTime);
		focusPosition += delta;
	}

	private void HandleRotation()
	{
		float rotateDir = 0f;
		if (Input.GetKey(KeyCode.Q)) rotateDir -= 1f;
		if (Input.GetKey(KeyCode.E)) rotateDir += 1f;
		yawDegrees += rotateDir * rotateSpeed * Time.deltaTime;
	}

	private void HandleZoom()
	{
		float scroll = Input.mouseScrollDelta.y;
		if (Mathf.Abs(scroll) > 0.01f)
		{
			distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
		}
	}

	private void UpdateTransform()
	{
		Quaternion rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
		Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
		transform.position = focusPosition + offset;
		transform.rotation = rotation;
	}

	private void HandleUnitSelected(GameObject unit)
	{
		if (unit == null)
		{
			SetFollowTarget(null);
			return;
		}
		SetFollowTarget(unit.transform);
	}

	private static Vector2 ReadMoveInput()
	{
		float x = 0f;
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;

		float y = 0f;
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;

		return new Vector2(x, y);
	}

	private Vector2 ReadEdgePan()
	{
		if (edgeThickness <= 0) return Vector2.zero;
		Vector2 mp = Input.mousePosition;
		float x = 0f;
		float y = 0f;
		if (mp.x <= edgeThickness) x = -1f;
		else if (mp.x >= Screen.width - edgeThickness) x = 1f;
		if (mp.y <= edgeThickness) y = -1f;
		else if (mp.y >= Screen.height - edgeThickness) y = 1f;
		return new Vector2(x, y);
	}

	public void SetFollowTarget(Transform target)
	{
		followTarget = target;
		if (followTarget)
		{
			focusPosition = followTarget.position;
		}
	}
}

