using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ResponsiveScaler - Handles automatic scaling of game elements across different device screens
/// Attach this script to UI elements or game objects that need responsive scaling
/// </summary>
public class ResponsiveScaler : MonoBehaviour
{
    [Header("Reference Resolution")]
    [SerializeField] private float referenceWidth = 1920f;
    [SerializeField] private float referenceHeight = 1080f;

    [Header("Scaling Settings")]
    [SerializeField] private bool maintainWidth = true;
    [SerializeField] private bool maintainHeight = false;
    [SerializeField] private bool scaleWithScreen = true;
    [SerializeField][Range(0f, 1f)] private float widthOrHeightDominance = 0.5f;

    [Header("Components to Scale")]
    [SerializeField] private bool scaleRectTransform = true;
    [SerializeField] private bool scaleCanvasScaler = false;
    [SerializeField] private bool scaleCamera = false;

    private RectTransform rectTransform;
    private CanvasScaler canvasScaler;
    private Camera cam;
    private float initialScale;
    private Vector2 initialSize;

    private void Awake()
    {
        // Cache components
        rectTransform = GetComponent<RectTransform>();
        canvasScaler = GetComponent<CanvasScaler>();
        cam = GetComponent<Camera>();

        if (rectTransform != null)
        {
            initialSize = rectTransform.sizeDelta;
        }

        if (cam != null)
        {
            initialScale = cam.orthographicSize;
        }

        // Apply initial scaling
        ApplyScaling();
    }

    private void Update()
    {
        if (scaleWithScreen)
        {
            ApplyScaling();
        }
    }

    public void ApplyScaling()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate the scale ratio based on screen dimensions compared to reference resolution
        float widthRatio = screenWidth / referenceWidth;
        float heightRatio = screenHeight / referenceHeight;

        // Determine which ratio to use based on settings
        float ratio;

        if (maintainWidth && !maintainHeight)
        {
            ratio = widthRatio;
        }
        else if (maintainHeight && !maintainWidth)
        {
            ratio = heightRatio;
        }
        else
        {
            // Blend between width and height ratio based on dominance setting
            ratio = Mathf.Lerp(widthRatio, heightRatio, widthOrHeightDominance);
        }

        // Apply scaling to the appropriate components
        if (scaleRectTransform && rectTransform != null)
        {
            ScaleRectTransform(ratio);
        }

        if (scaleCanvasScaler && canvasScaler != null)
        {
            ScaleCanvasScaler();
        }

        if (scaleCamera && cam != null)
        {
            ScaleCamera(ratio);
        }
    }

    private void ScaleRectTransform(float ratio)
    {
        // Scale the size of the RectTransform
        rectTransform.sizeDelta = initialSize * ratio;

        // You can also scale other properties like anchored position if needed
        // rectTransform.anchoredPosition = initialPosition * ratio;
    }

    private void ScaleCanvasScaler()
    {
        // Adjust CanvasScaler based on the current screen size
        float currentAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceWidth / referenceHeight;

        if (currentAspect > referenceAspect)
        {
            // Width is the constraint
            canvasScaler.matchWidthOrHeight = 1f; // Match height
        }
        else
        {
            // Height is the constraint
            canvasScaler.matchWidthOrHeight = 0f; // Match width
        }
    }

    private void ScaleCamera(float ratio)
    {
        // Adjust orthographic camera size to maintain view
        if (cam.orthographic)
        {
            cam.orthographicSize = initialScale / ratio;
        }
        else
        {
            // For perspective cameras, you might want to adjust field of view
            // cam.fieldOfView = initialFieldOfView / ratio;
        }
    }

    // Reapply scaling when device orientation changes
    private void OnRectTransformDimensionsChange()
    {
        if (scaleWithScreen)
        {
            ApplyScaling();
        }
    }

    // Public method to force rescaling
    public void ForceRescale()
    {
        ApplyScaling();
    }
}