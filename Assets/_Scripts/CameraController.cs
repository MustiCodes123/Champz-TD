using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("How fast the camera zooms in and out.")]
    [SerializeField] private float zoomSpeed = 5f;
    [Tooltip("The closest the camera can zoom in.")]
    [SerializeField] private float minZoom = 2f;
    [Tooltip("The furthest the camera can zoom out.")]
    [SerializeField] private float maxZoom = 15f;

    [Header("Movement Bounds")]
    [Tooltip("The leftmost X position the camera can move to.")]
    [SerializeField] private float minX = -10f;
    [Tooltip("The rightmost X position the camera can move to.")]
    [SerializeField] private float maxX = 10f;
    [Tooltip("The bottommost Y position the camera can move to.")]
    [SerializeField] private float minY = -10f;
    [Tooltip("The topmost Y position the camera can move to.")]
    [SerializeField] private float maxY = 10f;

    private Camera cam;
    private Vector3 dragOrigin;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Safety check to ensure the camera is Orthographic (required for 2D)
        if (!cam.orthographic)
        {
            Debug.LogWarning("Camera is not set to Orthographic! Switching it now.");
            cam.orthographic = true;
        }
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandleMouseDrag();
        ClampCameraPosition();
    }

    private void HandleZoom()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        // Only zoom if the mouse wheel is moving
        if (scrollData != 0f)
        {
            float targetSize = cam.orthographicSize;
            
            // Invert scrollData so scrolling up zooms IN (makes size smaller)
            targetSize -= scrollData * zoomSpeed;
            
            // Clamp the zoom value between min and max
            cam.orthographicSize = Mathf.Clamp(targetSize, minZoom, maxZoom);
        }
    }

    private void HandleMouseDrag()
    {
        // 1 = Right Mouse Button. Use 0 for Left, 2 for Middle.
        if (Input.GetMouseButtonDown(1))
        {
            // Capture the world point where we first clicked
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            // Calculate the distance between the drag origin and the current mouse position
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

            // Move the camera by that difference
            // We do not change the Z axis to avoid clipping issues
            transform.position += new Vector3(difference.x, difference.y, 0);
        }
    }

    private void ClampCameraPosition()
    {
        // Get current position
        Vector3 clampedPosition = transform.position;

        // Restrict X and Y within the min/max bounds
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

        // Apply changes
        transform.position = clampedPosition;
    }

    // This draws a green box in the Scene view to help you visualize the bounds
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        // Calculate center and size of the bounds box based on min/max
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        float sizeX = Mathf.Abs(maxX - minX);
        float sizeY = Mathf.Abs(maxY - minY);

        Gizmos.DrawWireCube(new Vector3(centerX, centerY, 0), new Vector3(sizeX, sizeY, 1));
    }
}