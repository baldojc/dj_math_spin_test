using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiskRotation : MonoBehaviour
{
    // References
    public GameManager gameManager;
    public bool isLeftDisk = true;

    // Rotation properties
    private float currentRotation = 0f;
    private bool isDragging = false;
    private Vector2 lastPointerPosition;

    // Disk properties
    public float snapAngle = 72f; // 360 / 5 numbers = 72 degrees per snap
    private int currentNumberIndex = 0;

    // Text references
    public TextMeshPro[] numberTexts = new TextMeshPro[5];

    void Start()
    {
        Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Start called");

        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): GameManager not found!");
            }
        }

        // Find text references if not assigned
        FindTextReferences();

        // Initialize disk at 0 rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);
        currentRotation = 0f;

        // Make sure this disk has a collider for input detection
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): No Collider2D found, adding CircleCollider2D");
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 2.5f; // Adjust based on your disk size
        }

        // Notify GameManager of initial position
        UpdateSelectedNumber();
    }

    void Update()
    {
        // Handle keyboard input for testing
        HandleKeyboardInput();

        // Handle touch input for mobile
        HandleTouchInput();

        // Handle mouse input for PC testing
        HandleMouseInput();
    }

    void HandleKeyboardInput()
    {
        if (!isDragging) // Don't handle keyboard if currently dragging
        {
            if (isLeftDisk)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RotateCounterClockwise();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    RotateClockwise();
                }
            }
            else // Right disk
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    RotateCounterClockwise();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    RotateClockwise();
                }
            }
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsPointerOnDisk(touchWorldPos))
                    {
                        isDragging = true;
                        lastPointerPosition = touchWorldPos;
                        Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Touch began");
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 currentTouchPos = touchWorldPos;
                        RotateDiskCircular(lastPointerPosition, currentTouchPos);
                        lastPointerPosition = currentTouchPos;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging)
                    {
                        isDragging = false;
                        SnapToNearestPosition();
                        Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Touch ended, snapped to index {currentNumberIndex}");
                    }
                    break;
            }
        }
    }

    void HandleMouseInput()
    {
        // Only handle mouse input if there are no touches (prevent double input on touch devices)
        if (Input.touchCount == 0)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Mouse button down - start dragging
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                if (IsPointerOnDisk(mouseWorldPos))
                {
                    isDragging = true;
                    lastPointerPosition = mouseWorldPos;
                    Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Mouse drag began");
                }
            }

            // Mouse move while dragging
            if (isDragging && Input.GetMouseButton(0))
            {
                RotateDiskCircular(lastPointerPosition, mouseWorldPos);
                lastPointerPosition = mouseWorldPos;
            }

            // Mouse button up - end dragging
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                SnapToNearestPosition();
                Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Mouse drag ended, snapped to index {currentNumberIndex}");
            }
        }
    }

    bool IsPointerOnDisk(Vector2 pointerPos)
    {
        Collider2D col = GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(pointerPos);
    }

    void RotateDiskCircular(Vector2 lastPos, Vector2 currentPos)
    {
        Vector2 center = transform.position;
        Vector2 from = lastPos - center;
        Vector2 to = currentPos - center;

        // Only rotate if vectors have some length to prevent extreme jumps
        if (from.magnitude > 0.1f && to.magnitude > 0.1f)
        {
            float angle = Vector2.SignedAngle(from, to);

            // Prevent extreme rotation jumps
            if (Mathf.Abs(angle) < 30f)
            {
                currentRotation += angle;
                transform.rotation = Quaternion.Euler(0, 0, currentRotation);

                // Calculate the current index based on rotation
                CalculateCurrentIndex();
            }
        }
    }

    void CalculateCurrentIndex()
    {
        // Normalize rotation to 0-360
        float normalizedRotation = currentRotation % 360;
        if (normalizedRotation < 0) normalizedRotation += 360;

        // Calculate index (5 positions around the disk)
        currentNumberIndex = Mathf.RoundToInt(normalizedRotation / snapAngle) % 5;
        if (currentNumberIndex < 0) currentNumberIndex += 5;

        // Update the GameManager with current selection
        UpdateSelectedNumber();
    }

    void SnapToNearestPosition()
    {
        // Calculate the nearest snap angle
        float targetRotation = currentNumberIndex * snapAngle;

        // Smoothly rotate to that position
        StartCoroutine(SmoothRotateToTarget(targetRotation));
    }

    IEnumerator SmoothRotateToTarget(float targetAngle)
    {
        float rotationSpeed = 360f; // degrees per second
        float startRotation = currentRotation;
        float t = 0;

        // Normalize current and target rotations to minimize rotation distance
        while (targetAngle - startRotation > 180) targetAngle -= 360;
        while (targetAngle - startRotation < -180) targetAngle += 360;

        float rotationDistance = Mathf.Abs(targetAngle - startRotation);

        while (t < 1)
        {
            t += Time.deltaTime * (rotationSpeed / Mathf.Max(10f, rotationDistance));
            currentRotation = Mathf.Lerp(startRotation, targetAngle, t);
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            yield return null;
        }

        // Ensure we're exactly at the target
        currentRotation = targetAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Final update of the selected number
        UpdateSelectedNumber();
    }

    void UpdateSelectedNumber()
    {
        if (gameManager != null)
        {
            if (isLeftDisk)
            {
                gameManager.SetSelectedLeftIndex(currentNumberIndex);
            }
            else
            {
                gameManager.SetSelectedRightIndex(currentNumberIndex);
            }
        }
        else
        {
            Debug.LogError($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): GameManager reference is null!");
        }
    }

    // Find text references if not assigned
    void FindTextReferences()
    {
        bool needToFindTexts = false;
        for (int i = 0; i < 5; i++)
        {
            if (numberTexts[i] == null)
            {
                needToFindTexts = true;
                break;
            }
        }

        if (needToFindTexts)
        {
            Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Finding text references");

            string prefix = isLeftDisk ? "leftDiskText" : "rightDiskText";

            for (int i = 0; i < 5; i++)
            {
                Transform textTransform = transform.Find($"{prefix}{i + 1}");
                if (textTransform != null)
                {
                    numberTexts[i] = textTransform.GetComponent<TextMeshPro>();
                    if (numberTexts[i] == null)
                    {
                        Debug.LogError($"DiskRotation: {prefix}{i + 1} found but has no TextMeshPro component!");
                    }
                }
                else
                {
                    Debug.LogError($"DiskRotation: Could not find {prefix}{i + 1} child transform!");
                }
            }
        }
    }

    // Manual rotation methods
    public void RotateClockwise()
    {
        currentRotation -= snapAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Update current index
        currentNumberIndex = (currentNumberIndex + 1) % 5;
        UpdateSelectedNumber();

        Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Rotated clockwise to index {currentNumberIndex}");
    }

    public void RotateCounterClockwise()
    {
        currentRotation += snapAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Update current index
        currentNumberIndex = (currentNumberIndex + 4) % 5; // +4 is same as -1 in modulo 5
        UpdateSelectedNumber();

        Debug.Log($"DiskRotation ({(isLeftDisk ? "Left" : "Right")}): Rotated counter-clockwise to index {currentNumberIndex}");
    }

    // Reset disk position
    public void ResetDisk()
    {
        currentRotation = 0;
        currentNumberIndex = 0;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        UpdateSelectedNumber();
    }

    // For testing - instantly set to a specific index position
    public void SetToIndex(int index)
    {
        if (index >= 0 && index < 5)
        {
            currentNumberIndex = index;
            currentRotation = index * snapAngle;
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            UpdateSelectedNumber();
        }
    }
}