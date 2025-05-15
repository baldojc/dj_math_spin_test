using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Main Panels
    public GameObject mainMenuPanel;
    public GameObject operationPickingPanel;
    public GameObject difficultyMenuPanel;
    public GameObject gameOverPanel;
    public GameObject howToPlayPanel;
    public GameObject loadingScreenPanel;

    public GameObject gamePanel;

    // Common game UI elements
    public GameObject pausePanel;
    public GameObject feedbackPanel;

    // Reference to Loading Screen Manager
    private LoadingScreenManager loadingScreenManager;

    // Feedback visuals
    public Sprite correctSprite;
    public Sprite incorrectSprite;

    // Current active game panel reference
    private GameObject currentGamePanel;

    // Current selection
    private string currentOperationStr;
    private string currentDifficultyStr;

    // Flag to track if game is paused
    private bool isGamePaused = false;

    // Audio UI References - Main Menu
    public Button mainMenuMusicButton;
    public Button mainMenuMuteButton;

    // Audio UI References - Pause Screen
    public Button pauseMusicButton;
    public Button pauseMuteButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate the loading screen panel reference first
        if (loadingScreenPanel == null)
        {
            // Try to find it by name in the scene
            loadingScreenPanel = GameObject.Find("LoadingScreenPanel");

            if (loadingScreenPanel == null)
            {
                Debug.LogError("LoadingScreenPanel is missing! Creating a temporary panel.");
                // Create a temporary panel - this is a fallback solution
                loadingScreenPanel = new GameObject("LoadingScreenPanel");
                loadingScreenPanel.transform.SetParent(transform, false);
            }
        }

        // Get the loading screen manager component
        loadingScreenManager = loadingScreenPanel.GetComponent<LoadingScreenManager>();
        if (loadingScreenManager == null)
        {
            loadingScreenManager = loadingScreenPanel.AddComponent<LoadingScreenManager>();
            Debug.LogWarning("LoadingScreenManager component was missing from loadingScreenPanel. Added component automatically.");
        }

        ShowMainMenu();
        FindAllAudioButtons();
        SetupAllAudioButtons();
    }

    // Find all audio control buttons in the scene
    private void FindAllAudioButtons()
    {
        // Find main menu audio buttons
        if (mainMenuPanel != null)
        {
            mainMenuMusicButton = mainMenuPanel.transform.Find("music_button")?.GetComponent<Button>();
            mainMenuMuteButton = mainMenuPanel.transform.Find("mute_button")?.GetComponent<Button>();
        }

        // Find pause panel audio buttons
        if (pausePanel != null)
        {
            pauseMusicButton = pausePanel.transform.Find("music_button")?.GetComponent<Button>();
            pauseMuteButton = pausePanel.transform.Find("mute_button")?.GetComponent<Button>();
        }
    }

    // Set up all audio button listeners
    private void SetupAllAudioButtons()
    {
        // Set up main menu audio buttons
        if (mainMenuMusicButton != null)
        {
            mainMenuMusicButton.onClick.RemoveAllListeners();
            mainMenuMusicButton.onClick.AddListener(OnMusicButtonClicked);
        }

        if (mainMenuMuteButton != null)
        {
            mainMenuMuteButton.onClick.RemoveAllListeners();
            mainMenuMuteButton.onClick.AddListener(OnMuteButtonClicked);
        }

        // Set up pause menu audio buttons
        if (pauseMusicButton != null)
        {
            pauseMusicButton.onClick.RemoveAllListeners();
            pauseMusicButton.onClick.AddListener(OnMusicButtonClicked);
        }

        if (pauseMuteButton != null)
        {
            pauseMuteButton.onClick.RemoveAllListeners();
            pauseMuteButton.onClick.AddListener(OnMuteButtonClicked);
        }
    }

    public void ShowMainMenu()
    {
        // Play menu music when returning to main menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        HideAllPanels();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseTimer();
        }

        ToggleHUD(false); // Hide the HUD in main menu
        mainMenuPanel.SetActive(true);
        Time.timeScale = 1;
        isGamePaused = false;
    }

    public void ShowHowToPlay()
    {
        HideAllPanels();
        howToPlayPanel.SetActive(true);
        Debug.Log("How To Play panel activated");
    }

    public void ShowOperationMenu()
    {
        HideAllPanels();
        operationPickingPanel.SetActive(true);
    }

    public void SelectOperation(string operation)
    {
        currentOperationStr = operation.ToLower();
        ShowDifficultyMenu();
    }

    public void ShowDifficultyMenu()
    {
        HideAllPanels();
        difficultyMenuPanel.SetActive(true);
    }

    public void SelectDifficulty(string difficulty)
    {
        currentDifficultyStr = difficulty.ToLower();
        StartGameWithCurrentSelections();
    }

    public void StartGameWithCurrentSelections()
    {
        // Add null checks for currentOperationStr and currentDifficultyStr
        if (string.IsNullOrEmpty(currentOperationStr))
            currentOperationStr = "addition";
        if (string.IsNullOrEmpty(currentDifficultyStr))
            currentDifficultyStr = "easy";

        // Convert string selections to enum values
        GameManager.Operation operation = GetOperationFromString(currentOperationStr);
        GameManager.Difficulty difficulty = GetDifficultyFromString(currentDifficultyStr);

        // Set the operation and difficulty in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetOperation(operation);
            GameManager.Instance.SetDifficulty(difficulty);
        }

        // Show loading screen before starting the game
        ShowLoadingScreen();
    }

    // Updated method to show loading screen
    private void ShowLoadingScreen()
    {
        HideAllPanels();

        // Play loading sound if you have one
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Button");
        }

        // Validate loading screen panel
        if (loadingScreenPanel == null)
        {
            Debug.LogError("Loading screen panel reference lost! Trying to find it.");
            loadingScreenPanel = GameObject.Find("LoadingScreenPanel");

            // If we still can't find it, create a fallback panel
            if (loadingScreenPanel == null)
            {
                Debug.LogError("Creating fallback loading screen panel");
                loadingScreenPanel = new GameObject("LoadingScreenPanel");
                loadingScreenPanel.transform.SetParent(transform, false);
                loadingScreenManager = loadingScreenPanel.AddComponent<LoadingScreenManager>();
            }
        }

        // Verify the loading screen manager component
        if (loadingScreenManager == null)
        {
            loadingScreenManager = loadingScreenPanel.GetComponent<LoadingScreenManager>();

            if (loadingScreenManager == null)
            {
                Debug.LogError("LoadingScreenManager component missing, adding it now");
                loadingScreenManager = loadingScreenPanel.AddComponent<LoadingScreenManager>();
            }
        }

        // Ensure the panel is active before starting the coroutine
        loadingScreenPanel.SetActive(true);

        Debug.Log("Starting loading screen coroutine");

        // Start the loading coroutine
        StartCoroutine(loadingScreenManager.ShowLoadingScreen(StartGame));
    }

    private GameManager.Operation GetOperationFromString(string operationStr)
    {
        switch (operationStr.ToLower())
        {
            case "addition": return GameManager.Operation.Addition;
            case "subtraction": return GameManager.Operation.Subtraction;
            case "multiplication": return GameManager.Operation.Multiplication;
            case "division": return GameManager.Operation.Division;
            default: return GameManager.Operation.Addition;
        }
    }

    private GameManager.Difficulty GetDifficultyFromString(string difficultyStr)
    {
        switch (difficultyStr.ToLower())
        {
            case "easy": return GameManager.Difficulty.Easy;
            case "medium": return GameManager.Difficulty.Medium;
            case "hard": return GameManager.Difficulty.Hard;
            default: return GameManager.Difficulty.Easy;
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game after loading screen");

        // Play gameplay music when starting the game
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }

        HideAllPanels();
        ToggleHUD(true);

        gamePanel.SetActive(true);
        SetupDisksInCurrentPanel();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GenerateTargetNumber();
            GameManager.Instance.ResetTimer();
            GameManager.Instance.ResumeTimer();
        }

        isGamePaused = false;
    }

    private GameObject GetGamePanelForOperationAndDifficulty(string operation, string difficulty)
    {
        return null;
    }

    private void SetupDisksInCurrentPanel()
    {
        DiskRotation[] disks = gamePanel.GetComponentsInChildren<DiskRotation>(true);
        foreach (DiskRotation disk in disks)
        {
            disk.ResetDiskPosition();
            disk.RefreshDisk();
        }
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        pausePanel.SetActive(isGamePaused);

        if (gamePanel != null)
            gamePanel.SetActive(!isGamePaused);

        Time.timeScale = isGamePaused ? 0 : 1;

        if (isGamePaused)
        {
            // Pause the music when entering pause menu
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PauseMusic();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseTimer();
            }
        }
        else
        {
            // Resume the music when exiting pause menu
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResumeMusic();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeTimer();
            }
        }
    }

    public void ShowFeedback(bool isCorrect)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);

            if (isCorrect)
            {
                Transform correctImage = feedbackPanel.transform.Find("correct");
                Transform incorrectImage = feedbackPanel.transform.Find("incorrect");
                correctImage.gameObject.SetActive(true);
                incorrectImage.gameObject.SetActive(false);
                AudioManager.Instance.PlaySound("Correct");
                AudioManager.Instance.PlaySound("Cheer", pitch: Random.Range(0.95f, 1.05f));
            }
            else
            {
                Transform correctImage = feedbackPanel.transform.Find("correct");
                Transform incorrectImage = feedbackPanel.transform.Find("incorrect");
                correctImage.gameObject.SetActive(false);
                incorrectImage.gameObject.SetActive(true);
                AudioManager.Instance.PlaySound("Incorrect");
                AudioManager.Instance.PlaySound("Scratch", pitch: 0.8f);
            }

            Invoke("HideFeedback", 1.0f);
        }
    }

    public void ShowGameOver(int finalScore, int highScore)
    {
        if (gameOverPanel != null)
        {
            HideAllPanels();
            ToggleHUD(false);
            gameOverPanel.SetActive(true);

            // Find and update score texts in the game over panel
            TMPro.TextMeshProUGUI finalScoreText = gameOverPanel.transform.Find("final_score")?.GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI highScoreText = gameOverPanel.transform.Find("high_score")?.GetComponent<TMPro.TextMeshProUGUI>();

            if (finalScoreText != null)
                finalScoreText.text = "Score: " + finalScore;

            if (highScoreText != null)
            {
                // Include operation and difficulty in high score text
                string operationName = GameManager.Instance.currentOperation.ToString();
                string difficultyName = GameManager.Instance.currentDifficulty.ToString();
                highScoreText.text = $"High Score ({operationName} {difficultyName}): {highScore}";
            }

            isGamePaused = false;
        }
    }

    private void HideFeedback()
    {
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (operationPickingPanel != null) operationPickingPanel.SetActive(false);
        if (difficultyMenuPanel != null) difficultyMenuPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);

        if (gamePanel != null) gamePanel.SetActive(false);

        if (pausePanel != null) pausePanel.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // Button click handlers
    public void OnStartButtonClicked()
    {
        ShowOperationMenu();
    }

    public void OnOperationButtonClicked(string operation)
    {
        SelectOperation(operation);
    }

    public void OnDifficultyButtonClicked(string difficulty)
    {
        SelectDifficulty(difficulty);
    }

    public void OnCheckAnswerButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            // Check the answer
            bool wasCorrect = GameManager.Instance.CheckAnswer();

            // Show feedback
            ShowFeedback(wasCorrect);
        }
    }

    public void OnBackButtonClicked()
    {
        if (operationPickingPanel.activeSelf)
        {
            ShowMainMenu();
        }
        else if (difficultyMenuPanel.activeSelf)
        {
            ShowOperationMenu();
        }
        else if (pausePanel.activeSelf)
        {
            TogglePause();
        }
        else if (howToPlayPanel.activeSelf)
        {
            ShowMainMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }

    public void OnExitToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseTimer();
        }

        // Stop the current gameplay music and switch to menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        ShowMainMenu();
    }

    public void OnRestartButtonClicked()
    {
        // Make sure we unpause the game first
        if (pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
            isGamePaused = false;
        }

        // Check if operation and difficulty are set, if not use defaults
        if (string.IsNullOrEmpty(currentOperationStr))
            currentOperationStr = "addition";
        if (string.IsNullOrEmpty(currentDifficultyStr))
            currentDifficultyStr = "easy";

        // Show loading screen before restarting
        ShowLoadingScreen();
    }

    public void ToggleHUD(bool show)
    {
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null)
        {
            Transform hudTransform = mainCanvas.transform.Find("HUD");
            if (hudTransform != null)
            {
                hudTransform.gameObject.SetActive(show);
                Debug.Log("HUD visibility set to: " + show);
            }
            else
            {
                Debug.LogError("HUD object not found in the scene hierarchy!");
            }
        }
    }

    // Audio Controls Methods
    public void OnMusicButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EnableMusic();
        }
    }

    public void OnMuteButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.DisableMusic();
        }
    }
}