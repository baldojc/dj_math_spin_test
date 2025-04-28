using UnityEngine;

public class DiskRotation : MonoBehaviour
{
    public bool isLeftDisk = true;
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;
    private Vector2 lastInputPosition;
    private GameManager gameManager;
    private AudioManager audioManager;

    // Sound effect parameters
    [Header("Sound Settings")]
    [SerializeField] private string rotationSoundName = "diskRotation";
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float minVolume = 0.3f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private float minPitch = 0.8f;

    // Smooth audio control variables
    private AudioSource rotationAudioSource;
    private bool isPlayingSound = false;
    private float targetVolume = 0f;
    private float targetPitch = 1f;
    private float smoothFactor = 8f; // Higher = faster audio transition
    private float rotationVelocity = 0f;
    private float rotationIdleTime = 0f;
    private const float ROTATION_TIMEOUT = 0.1f; // Time without rotation before fading out

    void Start()
    {
        // Get reference to the GameManager
        gameManager = GameManager.Instance;
        // Get reference to the AudioManager
        audioManager = AudioManager.Instance;

        // Setup dedicated audio source for smooth rotational audio
        SetupAudioSource();

        UpdateDiskNumbers();
        SelectCurrentNumber();
    }

    void SetupAudioSource()
    {
        // Create a dedicated audio source for rotation sound
        rotationAudioSource = gameObject.AddComponent<AudioSource>();

        // Find the sound in AudioManager
        if (audioManager != null)
        {
            AudioManager.Sound sound = audioManager.sounds.Find(s => s.name == rotationSoundName);
            if (sound != null && sound.clip != null)
            {
                rotationAudioSource.clip = sound.clip;
                rotationAudioSource.loop = true;
                rotationAudioSource.volume = 0f; // Start silent
                rotationAudioSource.pitch = 1f;
                rotationAudioSource.playOnAwake = false;
            }
            else
            {
                Debug.LogWarning($"Sound '{rotationSoundName}' not found in AudioManager or has no clip!");
            }
        }
    }

    void Update()
    {
        // Check for touch input first (mobile)
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }

        // Update sound smoothly
        UpdateRotationSound();
    }

    void UpdateRotationSound()
    {
        // If we're not rotating anymore, start counting idle time
        if (Mathf.Approximately(rotationVelocity, 0f))
        {
            rotationIdleTime += Time.deltaTime;
            if (rotationIdleTime > ROTATION_TIMEOUT)
            {
                // Fade out sound
                targetVolume = 0f;
            }
        }
        else
        {
            rotationIdleTime = 0f;
        }

        // Start playing if needed
        if (targetVolume > 0 && !isPlayingSound && rotationAudioSource.clip != null)
        {
            rotationAudioSource.Play();
            isPlayingSound = true;
        }

        // Smooth volume and pitch transitions
        rotationAudioSource.volume = Mathf.Lerp(rotationAudioSource.volume, targetVolume, Time.deltaTime * smoothFactor);
        rotationAudioSource.pitch = Mathf.Lerp(rotationAudioSource.pitch, targetPitch, Time.deltaTime * smoothFactor);

        // Stop sound if volume is very low
        if (isPlayingSound && rotationAudioSource.volume < 0.01f && targetVolume == 0f)
        {
            rotationAudioSource.Stop();
            isPlayingSound = false;
        }
    }

    // Update disk numbers when operation or difficulty changes
    public void UpdateDiskNumbers()
    {
        // Try to find GameManager if reference is null
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;

            // If still null, try to find directly
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

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
                // Start fading out rotation sound
                targetVolume = 0f;
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
            // Start fading out rotation sound
            targetVolume = 0f;
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

        // Store rotation velocity for sound effects
        rotationVelocity = angle / Time.deltaTime;

        // Apply rotation
        currentRotation += angle;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Update sound parameters based on rotation
        UpdateRotationSoundParameters(angle);
    }

    void UpdateRotationSoundParameters(float rotationDelta)
    {
        if (rotationAudioSource == null || rotationAudioSource.clip == null)
            return;

        float rotationSpeed = Mathf.Abs(rotationDelta);

        if (rotationSpeed > 0.1f)
        {
            // Calculate volume based on rotation speed
            float speedFactor = Mathf.Clamp01(rotationSpeed / 15f);

            // Direction affects volume and pitch
            if (rotationDelta > 0) // Counter-clockwise (left) rotation
            {
                // Higher volume for left rotation
                targetVolume = Mathf.Lerp(minVolume, maxVolume, speedFactor);
                // Higher pitch for left rotation
                targetPitch = Mathf.Lerp(1.0f, maxPitch, speedFactor);
            }
            else // Clockwise (right) rotation
            {
                // Lower volume for right rotation (calmer sound)
                targetVolume = Mathf.Lerp(minVolume, minVolume + (maxVolume - minVolume) * 0.5f, speedFactor);
                // Lower pitch for right rotation
                targetPitch = Mathf.Lerp(1.0f, minPitch, speedFactor);
            }
        }
    }

    void SelectCurrentNumber()
    {
        if (gameManager == null || numbers == null || numbers.Length == 0)
            return;

        int selectedIndex = Mathf.RoundToInt((360f - currentRotation) / (360f / numbers.Length)) % numbers.Length;
        selectedIndex = (selectedIndex + numbers.Length) % numbers.Length;

        if (isLeftDisk)
            gameManager.UpdateLeftNumber(numbers[selectedIndex]);
        else
            gameManager.UpdateRightNumber(numbers[selectedIndex]);
    }

    public void RefreshDisk()
    {
        UpdateDiskNumbers();
        SelectCurrentNumber();
    }

    public void ResetDiskPosition()
    {
        currentRotation = 0f;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        targetVolume = 0f;
        rotationVelocity = 0f;

        SelectCurrentNumber();
    }

    void OnDestroy()
    {
        if (rotationAudioSource != null)
            rotationAudioSource.Stop();
    }
}