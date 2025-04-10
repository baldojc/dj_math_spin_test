using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public GameManager gameManager;
    public bool isLeftDisk = true;
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;
    private Vector2 lastInputPosition;

    // For PC testing
    public bool enableMouseInput = true;

    // Rotation settings
    public float rotationSpeed = 1.0f; // Adjust to control sensitivity
    public float snapThreshold = 5.0f; // Degrees threshold for snapping

    void Start()
    {
        numbers = isLeftDisk ? new int[] { 3, 2, 8, 9, 7, 4 } : new int[] { 1, 6, 5, 2, 3, 9 };
    }

    void Update()
    {
        // Check for touch input first
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        // If no touch input and mouse input is enabled, handle mouse
        else if (enableMouseInput)
        {
            HandleMouse();
        }
    }

    void HandleTouch()
    {
        Touch touch = Input.GetTouch(0);
        Vector2 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (IsTouchOnThisDisk(touchWorldPos))
                {
                    isDragging = true;
                    lastInputPosition = touchWorldPos;
                }
                break;
            case TouchPhase.Moved:
                if (isDragging)
                {
                    RotateDiskCircular(touchWorldPos);
                }
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                SnapToNearest();
                break;
        }
    }

    void HandleMouse()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOnThisDisk(mouseWorldPos))
            {
                isDragging = true;
                lastInputPosition = mouseWorldPos;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                RotateDiskCircular(mouseWorldPos);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            SnapToNearest();
        }
    }

    bool IsTouchOnThisDisk(Vector2 touchPos)
    {
        Collider2D col = GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(touchPos);
    }

    void RotateDiskCircular(Vector2 currentPos)
    {
        Vector2 center = transform.position;

        // Calculate vectors from center to touch positions
        Vector2 previousVector = lastInputPosition - center;
        Vector2 currentVector = currentPos - center;

        // Skip if touch too close to center to avoid erratic behavior
        if (previousVector.magnitude < 0.1f || currentVector.magnitude < 0.1f)
            return;

        // Calculate the angle between the vectors
        float angle = Vector2.SignedAngle(previousVector, currentVector);

        // Apply rotation based on angle and sensitivity
        currentRotation += angle * rotationSpeed;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Update game manager with the current number
        UpdateSelectedNumber();

        // Update last position for next frame
        lastInputPosition = currentPos;
    }

    void UpdateSelectedNumber()
    {
        // Calculate which number is currently at the "top" position
        float singleSectionAngle = 360f / numbers.Length;
        int selectedIndex = Mathf.FloorToInt(((currentRotation % 360) + 360) % 360 / singleSectionAngle);
        selectedIndex = (numbers.Length - selectedIndex) % numbers.Length;

        // Update the game manager
        if (isLeftDisk)
            gameManager.UpdateLeftNumber(numbers[selectedIndex]);
        else
            gameManager.UpdateRightNumber(numbers[selectedIndex]);
    }

    void SnapToNearest()
    {
        // Calculate the angle of a single section
        float singleSectionAngle = 360f / numbers.Length;

        // Calculate how far we are from the nearest snap point
        float normalizedRotation = ((currentRotation % 360) + 360) % 360;
        float targetRotation = Mathf.Round(normalizedRotation / singleSectionAngle) * singleSectionAngle;

        // Snap to the nearest position if we're close enough
        if (Mathf.Abs(normalizedRotation - targetRotation) < snapThreshold)
        {
            currentRotation = targetRotation + (currentRotation - normalizedRotation);
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            UpdateSelectedNumber();
        }
    }

    // Optional: Add visual feedback for testing
    void OnDrawGizmos()
    {
        if (Application.isPlaying && isDragging)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Draw lines to represent the current selection
            float singleSectionAngle = 360f / (numbers != null ? numbers.Length : 6);
            Vector3 direction = Quaternion.Euler(0, 0, -currentRotation) * Vector3.up;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction);
        }
    }
}