using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public GameManager gameManager;
    public bool isLeftDisk = true;
    private float rotationSpeed = 0.5f; // Increased rotation speed
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;

    void Start()
    {
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
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            Debug.Log("Touch at: " + touchPosition); // Debug touch position

            RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("Hit object: " + hit.collider.gameObject.name);

                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                }

                if (isDragging && touch.phase == TouchPhase.Moved)
                {
                    float rotationInput = touch.deltaPosition.x * rotationSpeed;
                    transform.Rotate(0, 0, -rotationInput); // Apply rotation
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
            }
            else
            {
                Debug.Log("No object hit");
            }
        }
    }
}
