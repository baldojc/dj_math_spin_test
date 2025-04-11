using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public bool isLeftDisk = true;
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;
    private Vector2 lastInputPosition;
    private GameManager gameManager;

    void Start()
    {
        // Get reference to the GameManager
        gameManager = GameManager.Instance;

        // Initialize with the correct numbers from GameManager
        UpdateDiskNumbers();

        // Set initial selected numbers
        SelectCurrentNumber();
    }

    void Update()
    {
        // Check for touch input first (mobile)
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        // Check for mouse input (PC)
        else
        {
            HandleMouse();
        }
    }

    // Update disk numbers when operation or difficulty changes
    public void UpdateDiskNumbers()
    {
        if (gameManager != null)
        {
            numbers = gameManager.GetDiskNumbers(isLeftDisk);
        }
        else
        {
            Debug.LogError("GameManager reference not found!");
            // Fallback numbers
            numbers = isLeftDisk ? new int[] { 1, 2, 3, 4, 5, 6 } : new int[] { 1, 2, 3, 4, 5, 6 };
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
                    Vector2 currentTouchPos = touchWorldPos;
                    RotateDiskCircular(lastInputPosition, currentTouchPos);
                    lastInputPosition = currentTouchPos;
                }
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                SelectCurrentNumber();
                break;
        }
    }

    void HandleMouse()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Mouse button down (equivalent to touch began)
        if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOnThisDisk(mouseWorldPos))
            {
                isDragging = true;
                lastInputPosition = mouseWorldPos;
            }
        }
        // Mouse drag (equivalent to touch moved)
        else if (Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                Vector2 currentMousePos = mouseWorldPos;
                RotateDiskCircular(lastInputPosition, currentMousePos);
                lastInputPosition = currentMousePos;
            }
        }
        // Mouse button up (equivalent to touch ended)
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            SelectCurrentNumber();
        }
    }

    bool IsTouchOnThisDisk(Vector2 inputPos)
    {
        Collider2D col = GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(inputPos);
    }

    void RotateDiskCircular(Vector2 lastPos, Vector2 currentPos)
    {
        Vector2 center = transform.position;
        Vector2 from = lastPos - center;
        Vector2 to = currentPos - center;
        float angle = Vector2.SignedAngle(from, to);
        currentRotation += angle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    // Select the number based on current rotation
    void SelectCurrentNumber()
    {
        if (gameManager == null || numbers == null || numbers.Length == 0)
            return;

        // Calculate which number is currently at the top position
        int selectedIndex = Mathf.RoundToInt((360f - currentRotation) / (360f / numbers.Length)) % numbers.Length;
        selectedIndex = (selectedIndex + numbers.Length) % numbers.Length;

        // Update the selected number in GameManager
        if (isLeftDisk)
            gameManager.UpdateLeftNumber(numbers[selectedIndex]);
        else
            gameManager.UpdateRightNumber(numbers[selectedIndex]);
    }

    // Add this method to simplify updating for changes in operation/difficulty
    public void RefreshDisk()
    {
        UpdateDiskNumbers();
        SelectCurrentNumber();
    }
}