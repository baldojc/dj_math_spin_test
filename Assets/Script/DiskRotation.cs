using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public GameManager gameManager;
    public bool isLeftDisk = true;

    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;

    private Vector2 lastTouchPosition;
    private Collider2D diskCollider;

    void Start()
    {
        numbers = isLeftDisk ? new int[] { 3, 2, 8, 9, 7, 4 } : new int[] { 1, 6, 5, 2, 3, 9 };
        diskCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        Vector2 inputPos = Vector2.zero;

        if (Input.touchCount > 0) // For touch input (mobile)
        {
            Touch touch = Input.GetTouch(0);
            inputPos = Camera.main.ScreenToWorldPoint(touch.position);
        }
        else if (Input.GetMouseButton(0)) // For mouse input (Unity Editor)
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (inputPos != Vector2.zero)
        {
            switch (Input.touchCount > 0 ? Input.GetTouch(0).phase : (Input.GetMouseButtonDown(0) ? TouchPhase.Began : TouchPhase.Moved))
            {
                case TouchPhase.Began:
                    if (IsTouchOnThisDisk(inputPos))
                    {
                        isDragging = true;
                        lastTouchPosition = inputPos;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        RotateDiskCircular(lastTouchPosition, inputPos);
                        lastTouchPosition = inputPos;
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
        return diskCollider != null && diskCollider.OverlapPoint(touchPos);
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
}
