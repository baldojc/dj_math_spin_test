using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public GameManager gameManager;
    public bool isLeftDisk = true;  // Set in Inspector
    private float rotationSpeed = 0.2f;
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;

    void Start()
    {
        // Assign numbers based on whether this is the left or right disk
        numbers = isLeftDisk ? new int[] { 3, 4 } : new int[] { 1, 2 };
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
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (touch.phase == TouchPhase.Began)
                    isDragging = true;

                if (isDragging && touch.phase == TouchPhase.Moved)
                {
                    float rotationInput = touch.deltaPosition.x * rotationSpeed;
                    currentRotation += rotationInput;
                    currentRotation = Mathf.Repeat(currentRotation, 360f);
                    transform.rotation = Quaternion.Euler(0, 0, currentRotation);

                    int selectedIndex = Mathf.RoundToInt(currentRotation / 180f) % 2;
                    if (isLeftDisk)
                        gameManager.UpdateLeftNumber(numbers[selectedIndex]);
                    else
                        gameManager.UpdateRightNumber(numbers[selectedIndex]);
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    isDragging = false;
            }
        }
    }
}
