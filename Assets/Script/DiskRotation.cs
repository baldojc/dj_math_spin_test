using UnityEngine;
using TMPro; // Import the TMPro namespace

public class DiskRotation : MonoBehaviour
{
    public GameManager gameManager;
    public bool isLeftDisk = true;

    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;

    private Vector2 lastTouchPosition;

    void Start()
    {
        // Adjust the number range based on difficulty
        SetNumbersForDifficulty();
    }

    void Update()
    {
        HandleTouch();
    }

    void HandleTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchOnThisDisk(touchWorldPos))
                    {
                        isDragging = true;
                        lastTouchPosition = touchWorldPos;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 currentTouchPos = touchWorldPos;
                        RotateDiskCircular(lastTouchPosition, currentTouchPos);
                        lastTouchPosition = currentTouchPos;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    bool IsTouchOnThisDisk(Vector2 touchPos)
    {
        Collider2D col = GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(touchPos);
    }

    void RotateDiskCircular(Vector2 lastPos, Vector2 currentPos)
    {
        Vector2 center = transform.position;

        Vector2 from = lastPos - center;
        Vector2 to = currentPos - center;

        float angle = Vector2.SignedAngle(from, to);

        currentRotation += angle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        int selectedIndex = Mathf.RoundToInt((360f - currentRotation) / (360f / numbers.Length)) % numbers.Length;
        selectedIndex = (selectedIndex + numbers.Length) % numbers.Length;

        if (isLeftDisk)
            gameManager.UpdateLeftNumber(numbers[selectedIndex]);
        else
            gameManager.UpdateRightNumber(numbers[selectedIndex]);
    }

    // Adjust number selection for difficulty
    void SetNumbersForDifficulty()
    {
        switch (gameManager.currentDifficulty)
        {
            case GameManager.Difficulty.Easy:
                numbers = isLeftDisk ? new int[] { 1, 2, 3, 4, 5 } : new int[] { 1, 2, 3, 4, 5 };
                break;
            case GameManager.Difficulty.Medium:
                numbers = isLeftDisk ? new int[] { 6, 7, 8, 9, 10 } : new int[] { 6, 7, 8, 9, 10 };
                break;
            case GameManager.Difficulty.Hard:
                numbers = isLeftDisk ? new int[] { 11, 12, 13, 14, 15 } : new int[] { 11, 12, 13, 14, 15 };
                break;
        }
    }
}
