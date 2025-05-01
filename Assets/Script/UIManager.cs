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

    // Game Panels - using a consistent naming convention
    // Addition panels
    public GameObject gamePanel_Addition_Easy;
    public GameObject gamePanel_Addition_Medium;
    public GameObject gamePanel_Addition_Hard;

    // Subtraction panels
    public GameObject gamePanel_Subtraction_Easy;
    public GameObject gamePanel_Subtraction_Medium;
    public GameObject gamePanel_Subtraction_Hard;

    // Multiplication panels
    public GameObject gamePanel_Multiplication_Easy;
    public GameObject gamePanel_Multiplication_Medium;
    public GameObject gamePanel_Multiplication_Hard;

    // Division panels
    public GameObject gamePanel_Division_Easy;
    public GameObject gamePanel_Division_Medium;
    public GameObject gamePanel_Division_Hard;

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

    #region Audio Controls
    [Header("Audio UI References")]
    public Button musicToggleButton;
    public GameObject volumeSliderPanel;
    public Slider volumeSlider;

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
        SetupVolumeControls();
    }

    public void ShowMainMenu()
    {
        AudioManager.Instance.PlayMenuMusic();
        HideAllPanels();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseTimer();
        }

        ToggleHUD(false); // Hide the HUD in main menu
        mainMenuPanel.SetActive(true);
        Time.timeScale = 1;
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
        AudioManager.Instance.PlayGameplayMusic();
        HideAllPanels();

        // Show the HUD for gameplay
        ToggleHUD(true);
        Debug.Log("Attempting to show HUD for gameplay");

        // Activate the specific game panel based on current selections
        currentGamePanel = GetGamePanelForOperationAndDifficulty(currentOperationStr, currentDifficultyStr);

        if (currentGamePanel != null)
        {
            currentGamePanel.SetActive(true);
            Debug.Log($"Activated game panel: {currentGamePanel.name}");

            // Find and initialize the disks in the current panel
            SetupDisksInCurrentPanel();

            // Always reset game state when starting a new game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GenerateTargetNumber();
                GameManager.Instance.ResetTimer();
                GameManager.Instance.ResumeTimer(); // Explicitly start the timer
            }
        }
        else
        {
            Debug.LogError($"Game panel not found for operation: {currentOperationStr}, difficulty: {currentDifficultyStr}");
        }
    }

    private GameObject GetGamePanelForOperationAndDifficulty(string operation, string difficulty)
    {
        // Standardized approach to get the correct panel
        operation = char.ToUpper(operation[0]) + operation.Substring(1).ToLower();
        difficulty = char.ToUpper(difficulty[0]) + difficulty.Substring(1).ToLower();

        switch (operation)
        {
            case "Addition":
                switch (difficulty)
                {
                    case "Easy": return gamePanel_Addition_Easy;
                    case "Medium": return gamePanel_Addition_Medium;
                    case "Hard": return gamePanel_Addition_Hard;
                }
                break;
            case "Subtraction":
                switch (difficulty)
                {
                    case "Easy": return gamePanel_Subtraction_Easy;
                    case "Medium": return gamePanel_Subtraction_Medium;
                    case "Hard": return gamePanel_Subtraction_Hard;
                }
                break;
            case "Multiplication":
                switch (difficulty)
                {
                    case "Easy": return gamePanel_Multiplication_Easy;
                    case "Medium": return gamePanel_Multiplication_Medium;
                    case "Hard": return gamePanel_Multiplication_Hard;
                }
                break;
            case "Division":
                switch (difficulty)
                {
                    case "Easy": return gamePanel_Division_Easy;
                    case "Medium": return gamePanel_Division_Medium;
                    case "Hard": return gamePanel_Division_Hard;
                }
                break;
        }

        return null;
    }

    private void SetupDisksInCurrentPanel()
    {
        if (currentGamePanel == null) return;

        // Find the disks in the current game panel
        DiskRotation[] disks = currentGamePanel.GetComponentsInChildren<DiskRotation>(true);

        foreach (DiskRotation disk in disks)
        {
            // Reset disk position
            disk.ResetDiskPosition();
            // Refresh the disk numbers based on current operation and difficulty
            disk.RefreshDisk();
        }

        Debug.Log($"Found and set up {disks.Length} disks in the current panel");
    }

    public void TogglePause()
    {
        bool isPaused = pausePanel.activeSelf;
        pausePanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1 : 0;

        // Add these lines:
        if (GameManager.Instance != null)
        {
            if (isPaused)
                GameManager.Instance.ResumeTimer();
            else
                GameManager.Instance.PauseTimer();
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

        if (gamePanel_Addition_Easy != null) gamePanel_Addition_Easy.SetActive(false);
        if (gamePanel_Addition_Medium != null) gamePanel_Addition_Medium.SetActive(false);
        if (gamePanel_Addition_Hard != null) gamePanel_Addition_Hard.SetActive(false);
        if (gamePanel_Subtraction_Easy != null) gamePanel_Subtraction_Easy.SetActive(false);
        if (gamePanel_Subtraction_Medium != null) gamePanel_Subtraction_Medium.SetActive(false);
        if (gamePanel_Subtraction_Hard != null) gamePanel_Subtraction_Hard.SetActive(false);
        if (gamePanel_Multiplication_Easy != null) gamePanel_Multiplication_Easy.SetActive(false);
        if (gamePanel_Multiplication_Medium != null) gamePanel_Multiplication_Medium.SetActive(false);
        if (gamePanel_Multiplication_Hard != null) gamePanel_Multiplication_Hard.SetActive(false);
        if (gamePanel_Division_Easy != null) gamePanel_Division_Easy.SetActive(false);
        if (gamePanel_Division_Medium != null) gamePanel_Division_Medium.SetActive(false);
        if (gamePanel_Division_Hard != null) gamePanel_Division_Hard.SetActive(false);

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
        // Determine which panel is active and go back accordingly
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
            TogglePause(); // Resume game
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

    
    public void SetupVolumeControls()
    {
        // Find references if not assigned
        if (musicToggleButton == null)
        {
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                Transform startMenuPanel = mainCanvas.transform.Find("Start_Main_Menu_Panel");
                if (startMenuPanel != null)
                {
                    musicToggleButton = startMenuPanel.Find("Music_Button")?.GetComponent<Button>();
                    volumeSliderPanel = startMenuPanel.Find("VolumeSliderPanel")?.gameObject;

                    if (volumeSliderPanel != null)
                    {
                        volumeSlider = volumeSliderPanel.GetComponentInChildren<Slider>();

                        // Set initial state
                        volumeSliderPanel.SetActive(false);

                        // Connect slider to volume control
                        if (volumeSlider != null && AudioManager.Instance != null)
                        {
                            volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
                            volumeSlider.onValueChanged.AddListener(delegate { AudioManager.Instance.SetMusicVolume(volumeSlider.value); });
                        }
                    }

                    // Set up music button
                    if (musicToggleButton != null)
                    {
                        musicToggleButton.onClick.RemoveAllListeners();
                        musicToggleButton.onClick.AddListener(ToggleVolumePanel);
                    }
                }
            }
        }
    }

    public void ToggleVolumePanel()
    {
        if (volumeSliderPanel != null)
        {
            bool isActive = volumeSliderPanel.activeSelf;
            volumeSliderPanel.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Volume slider panel reference not found!");
        }
    }

    #endregion










}
