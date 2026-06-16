using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Pivot")]
    public Vector3 pivotPoint = Vector3.zero;

    [Header("Distance & Zoom")]
    public float distance = 10.0f;
    // Note: Scroll values are much larger in the new system (often 120 per tick), so speed is drastically reduced.
    public float zoomSpeed = 0.02f;
    public float minDistance = 2.0f;
    public float maxDistance = 30.0f;

    [Header("Rotation Speed")]
    // Note: Mouse delta reads raw pixels in the new system, so these speeds are also reduced.
    public float xSpeed = 0.2f;
    public float ySpeed = 0.2f;

    [Header("Rotation Limits")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        // Safety check: ensure a mouse is actually connected
        if (Mouse.current == null) return;

        // 1. ROTATION: Check if Right Mouse Button is currently held down
        if (Mouse.current.rightButton.isPressed)
        {
            // Read raw mouse movement delta
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            x += mouseDelta.x * xSpeed;
            y -= mouseDelta.y * ySpeed;

            // Clamp vertical rotation
            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // 2. ZOOM: Read the scroll wheel Y axis
        float scroll = Mouse.current.scroll.y.ReadValue();
        if (scroll != 0.0f)
        {
            distance = Mathf.Clamp(distance - (scroll * zoomSpeed), minDistance, maxDistance);
        }

        // 3. APPLY POSITION
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + pivotPoint;

        transform.rotation = rotation;
        transform.position = position;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}