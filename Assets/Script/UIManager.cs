using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Main Panels
    public GameObject mainMenuPanel;
    public GameObject operationPickingPanel;
    public GameObject difficultyMenuPanel;
    public GameObject gameOverPanel;
    public GameObject howToPlayPanel;

    public GameObject gamePanel;

    // Common game UI elements
    public GameObject pausePanel;
    public GameObject feedbackPanel;

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

    // Audio UI References
    public Button musicButton;
    public Button muteButton;

    // Pause Screen Audio Controls
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
        ShowMainMenu();
        SetupAudioControls();
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

        // Start the game
        StartGame();
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

        // Toggle gamePanel visibility based on pause state
        if (gamePanel != null)
            gamePanel.SetActive(!isGamePaused);

        Time.timeScale = isGamePaused ? 0 : 1;

        // Set up audio controls if we're showing the pause panel
        if (isGamePaused)
        {
            SetupPauseScreenAudioControls();

            // Pause the timer when game is paused
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseTimer();
            }
        }
        else
        {
            // Resume the timer when game is unpaused
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

        // Restart current game with same operation and difficulty
        StartGameWithCurrentSelections();

        // Ensure the timer is reset and running
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetTimer();
            GameManager.Instance.ResumeTimer();
        }
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
    public void SetupAudioControls()
    {
        // Find references if not assigned
        if (musicButton == null || muteButton == null)
        {
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                Transform startMenuPanel = mainCanvas.transform.Find("Start_Main_Menu_Panel");
                if (startMenuPanel != null)
                {
                    musicButton = startMenuPanel.Find("Music_Button")?.GetComponent<Button>();
                    muteButton = startMenuPanel.Find("Mute_Button")?.GetComponent<Button>();

                    // Set up music button
                    if (musicButton != null)
                    {
                        musicButton.onClick.RemoveAllListeners();
                        musicButton.onClick.AddListener(OnMusicButtonClicked);
                    }

                    // Set up mute button
                    if (muteButton != null)
                    {
                        muteButton.onClick.RemoveAllListeners();
                        muteButton.onClick.AddListener(OnMuteButtonClicked);
                    }
                }
            }
        }
    }

    private void SetupPauseScreenAudioControls()
    {
        // Find references if not set
        if (pausePanel != null)
        {
            // Try to find buttons if not assigned
            if (pauseMusicButton == null)
                pauseMusicButton = pausePanel.transform.Find("Music_Button")?.GetComponent<Button>();

            if (pauseMuteButton == null)
                pauseMuteButton = pausePanel.transform.Find("Mute_Button")?.GetComponent<Button>();

            // Set up button listeners
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
    }

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