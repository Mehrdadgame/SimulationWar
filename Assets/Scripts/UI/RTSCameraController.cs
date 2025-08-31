using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float MoveSpeed = 10f;
    public float ZoomSpeed = 5f;
    public float MinZoom = 5f;
    public float MaxZoom = 30f;
    public float RotationSpeed = 100f;

    [Header("Bounds")]
    public float BoundarySize = 50f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        Vector3 newPosition = transform.position + direction * MoveSpeed * Time.deltaTime;

        // Clamp to boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, -BoundarySize, BoundarySize);
        newPosition.z = Mathf.Clamp(newPosition.z, -BoundarySize, BoundarySize);

        transform.position = newPosition;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 newPosition = transform.position + transform.forward * scroll * ZoomSpeed;

        newPosition.y = Mathf.Clamp(newPosition.y, MinZoom, MaxZoom);
        transform.position = newPosition;
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseX * RotationSpeed * Time.deltaTime, Space.World);
        }
    }
}