using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class DiskRotation : MonoBehaviour
{
    public bool isLeftDisk = true;
    private float currentRotation = 0f;
    private int[] numbers;
    private bool isDragging = false;
    private Vector2 lastInputPosition;
    private GameManager gameManager;
    private AudioManager audioManager;
    private Camera mainCamera;
    private InputAction pointerPosition;
    private InputAction pointerPress;

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
    private float smoothFactor = 8f; 
    private float rotationVelocity = 0f;
    private float rotationIdleTime = 0f;
    private const float ROTATION_TIMEOUT = 0.1f; 

    public TextMeshPro[] numberTexts;

    private void Awake()
    {
        
        mainCamera = Camera.main;

       
        pointerPosition = new InputAction("PointerPosition", binding: "<Pointer>/position");
        pointerPress = new InputAction("PointerPress", binding: "<Pointer>/press");

        // Register event callbacks
        pointerPress.performed += ctx => OnPointerDown();
        pointerPress.canceled += ctx => OnPointerUp();
    }

    private void OnEnable()
    {
       
        pointerPosition.Enable();
        pointerPress.Enable();

       
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
     
        pointerPosition.Disable();
        pointerPress.Disable();

        
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;

        if (EnhancedTouchSupport.enabled)
            EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance is null in Start()!");
        }
        else
        {
            Debug.Log("GameManager reference found successfully");
        }

        gameManager = GameManager.Instance;
        audioManager = AudioManager.Instance;
        mainCamera = Camera.main;
        SetupAudioSource();
        UpdateDiskNumbers();
        SelectCurrentNumber();
    }

    void SetupAudioSource()
    {
       
        rotationAudioSource = gameObject.AddComponent<AudioSource>();

     
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
       
        if (Touchscreen.current == null || !EnhancedTouchSupport.enabled)
        {
            HandlePointerInput();
        }

        // Update sound smoothly
        UpdateRotationSound();
    }

    private void HandlePointerInput()
    {
        if (isDragging)
        {
            Vector2 currentPointerPos = GetPointerWorldPosition();
            RotateDiskCircular(lastInputPosition, currentPointerPos);
            lastInputPosition = currentPointerPos;
        }
    }

    private Vector2 GetPointerWorldPosition()
    {
        Vector2 screenPosition = pointerPosition.ReadValue<Vector2>();
        return mainCamera.ScreenToWorldPoint(screenPosition);
    }

    private void OnPointerDown()
    {
        Vector2 pointerWorldPos = GetPointerWorldPosition();

        if (IsTouchOnThisDisk(pointerWorldPos))
        {
            isDragging = true;
            lastInputPosition = pointerWorldPos;
        }
    }

    private void OnPointerUp()
    {
        if (isDragging)
        {
            isDragging = false;
            SelectCurrentNumber();
            targetVolume = 0f; 
        }
    }

    // Enhanced Touch handlers for mobile
    private void OnFingerDown(Finger finger)
    {
        
        if (finger.index != 0) return;

        Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);

        if (IsTouchOnThisDisk(touchWorldPos))
        {
            isDragging = true;
            lastInputPosition = touchWorldPos;
        }
    }

    private void OnFingerMove(Finger finger)
    {
      
        if (finger.index != 0 || !isDragging) return;

        Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(finger.screenPosition);
        RotateDiskCircular(lastInputPosition, touchWorldPos);
        lastInputPosition = touchWorldPos;
    }

    private void OnFingerUp(Finger finger)
    {
       
        if (finger.index != 0 || !isDragging) return;

        isDragging = false;
        SelectCurrentNumber();
        targetVolume = 0f; 
    }

    void UpdateRotationSound()
    {
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

        if (targetVolume > 0 && !isPlayingSound && rotationAudioSource.clip != null)
        {
            rotationAudioSource.Play();
            isPlayingSound = true;
        }

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
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();

                if (gameManager == null)
                {
                    Debug.LogError("GameManager not found in scene!");
                    return;
                }
            }
        }

        try
        {
            numbers = gameManager.GetDiskNumbers(isLeftDisk);
            UpdateNumberDisplay();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update disk numbers: {e.Message}");
        }
    }
    private void UpdateNumberDisplay()
    {
        if (numberTexts == null || numberTexts.Length < 6)
        {
            Debug.LogError("NumberTexts not properly configured on disk!");
            return;
        }

        for (int i = 0; i < numbers.Length; i++)
        {
            if (numberTexts[i] != null)
            {
                numberTexts[i].text = numbers[i].ToString();
            }
            else
            {
                Debug.LogError($"TextMeshPro element {i} missing on disk!");
            }
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
            float speedFactor = Mathf.Clamp01(rotationSpeed / 15f);

            if (rotationDelta > 0) // Counter-clockwise (left) rotation
            {
                targetVolume = Mathf.Lerp(minVolume, maxVolume, speedFactor);
                targetPitch = Mathf.Lerp(1.0f, maxPitch, speedFactor);
            }
            else 
            {
                targetVolume = Mathf.Lerp(minVolume, minVolume + (maxVolume - minVolume) * 0.5f, speedFactor);
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