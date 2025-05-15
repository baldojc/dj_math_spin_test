using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Loading Screen Components")]
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;
    public GameObject loadingPanel;

    [Header("Loading Settings")]
    public float minLoadingTime = 1.5f; // Minimum time to show loading screen
    public float maxLoadingTime = 3.0f; // Maximum time to show loading screen

    [Header("Loading Tips")]
    public string[] loadingTips;

    private void Awake()
    {
        // Find references if they are null
        if (loadingPanel == null)
            loadingPanel = gameObject;

        if (loadingBar == null)
            loadingBar = GetComponentInChildren<Slider>();

        if (loadingText == null)
            loadingText = transform.Find("LoadingText")?.GetComponent<TextMeshProUGUI>();

        if (tipText == null)
            tipText = transform.Find("TipText")?.GetComponent<TextMeshProUGUI>();

        // Initialize the loading tips if none are set
        if (loadingTips == null || loadingTips.Length == 0)
        {
            loadingTips = new string[]
            {
                "Try to solve equations quickly to maximize your score!",
                "The harder the difficulty, the higher your score multiplier!",
                "Practice makes perfect!",
                "Division problems require careful thinking!",
                "Check your answer before submitting!"
            };
        }

        // Make sure we have all required components
        if (loadingBar == null)
            Debug.LogError("LoadingBar reference is missing in LoadingScreenManager!");

        if (loadingText == null)
            Debug.LogError("LoadingText reference is missing in LoadingScreenManager!");

        if (tipText == null)
            Debug.LogError("TipText reference is missing in LoadingScreenManager!");
    }

    /// <summary>
    /// Shows the loading screen and begins the loading process
    /// </summary>
    /// <param name="onLoadingComplete">Action to call when loading is complete</param>
    public IEnumerator ShowLoadingScreen(System.Action onLoadingComplete)
    {
        Debug.Log("Loading screen started");

        // Ensure the loading panel is active
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        else
            Debug.LogError("Loading panel reference is missing!");

        // Reset loading bar
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }
        else
        {
            Debug.LogError("Loading bar reference is missing!");
            // Try to find it if not set
            loadingBar = GetComponentInChildren<Slider>(true);
        }

        // Set initial loading text
        if (loadingText != null)
        {
            loadingText.text = "Loading... 0%";
        }
        else
        {
            Debug.LogError("Loading text reference is missing!");
            // Try to find it if not set
            loadingText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // Check tip text reference and set tip
        if (tipText != null)
        {
            // Pick a random tip
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        else
        {
            Debug.LogError("Tip text reference is missing!");
        }

        // Determine loading time (random between min and max)
        float loadingTime = Random.Range(minLoadingTime, maxLoadingTime);
        float startTime = Time.time;
        float elapsedTime = 0;

        Debug.Log($"Loading screen will display for {loadingTime} seconds");

        // While loading is not complete
        while (elapsedTime < loadingTime)
        {
            elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / loadingTime);

            // Update loading bar
            if (loadingBar != null)
                loadingBar.value = progress;

            // Update loading text
            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            yield return null;
        }

        // Ensure we reach 100%
        if (loadingBar != null)
            loadingBar.value = 1f;
        if (loadingText != null)
            loadingText.text = "Loading... 100%";

        Debug.Log("Loading screen reached 100%");

        // Small delay at 100%
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Loading screen complete, invoking callback");

        // Hide the loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        else
            Debug.LogError("Loading panel reference lost during loading!");

        // Call the completion callback
        onLoadingComplete?.Invoke();
    }
}