using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    // References to UI elements
    [Header("Game Panels")]
    public GameObject mainMenuPanel;
    public GameObject operationPickingPanel;
    public GameObject difficultyMenuPanel;
    public GameObject gameScenePanel;
    public GameObject gameOverPanel;
    public GameObject howToPlayPanel; // Added How To Play panel

    [Header("Game UI Elements")]
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI selectedNumberText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public Image operatorImage;
    public GameObject pauseScreen;

    [Header("Game Over UI Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Operator Sprites")]
    public Sprite additionSprite;
    public Sprite subtractionSprite;
    public Sprite multiplicationSprite;
    public Sprite divisionSprite;

    // References to manager objects
    private GameManager gameManager;
    private DiskManager diskManager;

    // Start is called before the first frame update
    void Start()
    {
        // Find manager references
        gameManager = FindObjectOfType<GameManager>();
        diskManager = FindObjectOfType<DiskManager>();

        // Initially show only the main menu
        ShowMainMenu();
    }

    // Show main menu panel
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        operationPickingPanel.SetActive(false);
        difficultyMenuPanel.SetActive(false);
        gameScenePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // Show operation picking panel
    public void ShowOperationPickingPanel()
    {
        mainMenuPanel.SetActive(false);
        operationPickingPanel.SetActive(true);
        difficultyMenuPanel.SetActive(false);
        gameScenePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // Show difficulty menu panel
    public void ShowDifficultyMenuPanel()
    {
        mainMenuPanel.SetActive(false);
        operationPickingPanel.SetActive(false);
        difficultyMenuPanel.SetActive(true);
        gameScenePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // Show game scene panel
    public void ShowGameScene()
    {
        mainMenuPanel.SetActive(false);
        operationPickingPanel.SetActive(false);
        difficultyMenuPanel.SetActive(false);
        gameScenePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        // Ensure pause screen is hidden
        ShowPauseScreen(false);
    }

    // Show game over panel
    public void ShowGameOverPanel(int finalScore, int highScore)
    {
        mainMenuPanel.SetActive(false);
        operationPickingPanel.SetActive(false);
        difficultyMenuPanel.SetActive(false);
        gameScenePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        // Update score texts
        finalScoreText.text = "Final Score: " + finalScore;
        highScoreText.text = "High Score: " + highScore;
    }

    // Show How To Play panel
    public void ShowHowToPlayPanel()
    {
        if (howToPlayPanel != null)
        {
            mainMenuPanel.SetActive(false);
            operationPickingPanel.SetActive(false);
            difficultyMenuPanel.SetActive(false);
            gameScenePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            howToPlayPanel.SetActive(true);
        }
    }

    // Show/hide pause screen
    public void ShowPauseScreen(bool show)
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(show);
        }
    }

    // Update the target number display
    public void UpdateTargetNumber(int number)
    {
        if (targetNumberText != null)
        {
            targetNumberText.text = "Target: " + number;
        }
    }

    // Update the selected number display
    public void UpdateSelectedNumber(int number)
    {
        if (selectedNumberText != null)
        {
            selectedNumberText.text = "Selected: " + number;
        }
    }

    // Update the timer display
    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(time);
        }
    }

    // Update the score display
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
    public void UpdateSelectedNumberDisplay(int leftNumber, int rightNumber, int result)
    {
        if (selectedNumberText != null)
        {
            string operatorSymbol = "+";

            // Determine which operator to display
            if (gameManager != null)
            {
                switch (gameManager.currentOperation)
                {
                    case GameManager.Operation.Addition:
                        operatorSymbol = "+";
                        break;
                    case GameManager.Operation.Subtraction:
                        operatorSymbol = "-";
                        break;
                    case GameManager.Operation.Multiplication:
                        operatorSymbol = "×";
                        break;
                    case GameManager.Operation.Division:
                        operatorSymbol = "÷";
                        break;
                }
            }

            selectedNumberText.text = $"Selected: {leftNumber} {operatorSymbol} {rightNumber} = {result}";
        }
    }
    // Set the operator image based on selected operation
    public void SetOperatorImage(GameManager.Operation operation)
    {
        if (operatorImage != null)
        {
            switch (operation)
            {
                case GameManager.Operation.Addition:
                    operatorImage.sprite = additionSprite;
                    break;

                case GameManager.Operation.Subtraction:
                    operatorImage.sprite = subtractionSprite;
                    break;

                case GameManager.Operation.Multiplication:
                    operatorImage.sprite = multiplicationSprite;
                    break;

                case GameManager.Operation.Division:
                    operatorImage.sprite = divisionSprite;
                    break;
            }
        }
    }

    // Operation selection buttons
    public void SelectAddition()
    {
        if (gameManager != null)
        {
            gameManager.currentOperation = GameManager.Operation.Addition;
        }
        ShowDifficultyMenuPanel();
    }

    public void SelectSubtraction()
    {
        if (gameManager != null)
        {
            gameManager.currentOperation = GameManager.Operation.Subtraction;
            // Make sure the operator image is updated immediately
            SetOperatorImage(GameManager.Operation.Subtraction);
        }
        ShowDifficultyMenuPanel();
    }

    public void SelectMultiplication()
    {
        if (gameManager != null)
        {
            gameManager.currentOperation = GameManager.Operation.Multiplication;
            SetOperatorImage(GameManager.Operation.Multiplication);

        }
        ShowDifficultyMenuPanel();
    }

    public void SelectDivision()
    {
        if (gameManager != null)
        {
            gameManager.currentOperation = GameManager.Operation.Division;
            SetOperatorImage(GameManager.Operation.Division);

        }
        ShowDifficultyMenuPanel();
    }

    // Difficulty selection buttons
    public void SelectEasy()
    {
        if (gameManager != null)
        {
            gameManager.currentDifficulty = GameManager.Difficulty.Easy;
            gameManager.InitializeGame(gameManager.currentOperation, GameManager.Difficulty.Easy);
        }
        ShowGameScene();
    }

    public void SelectMedium()
    {
        if (gameManager != null)
        {
            gameManager.currentDifficulty = GameManager.Difficulty.Medium;
            gameManager.InitializeGame(gameManager.currentOperation, GameManager.Difficulty.Medium);
        }
        ShowGameScene();
    }

    public void SelectHard()
    {
        if (gameManager != null)
        {
            gameManager.currentDifficulty = GameManager.Difficulty.Hard;
            gameManager.InitializeGame(gameManager.currentOperation, GameManager.Difficulty.Hard);
        }
        ShowGameScene();
    }

    // Game control buttons
    public void CheckAnswer()
    {
        if (gameManager != null)
        {
            gameManager.CheckAnswer();
        }
    }

    public void PauseGame()
    {
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    // Audio control
    public void MuteAudio()
    {
        if (diskManager != null)
        {
            diskManager.MuteAudio();
        }
    }

    public void UnmuteAudio()
    {
        if (diskManager != null)
        {
            diskManager.UnmuteAudio();
        }
    }

    public void ToggleAudio()
    {
        if (diskManager != null)
        {
            diskManager.ToggleMute();
        }
    }

    // Disk rotation controls
    public void RotateLeftDiskClockwise()
    {
        if (diskManager != null)
        {
            diskManager.RotateLeftDiskClockwise();
        }
    }

    public void RotateLeftDiskCounterClockwise()
    {
        if (diskManager != null)
        {
            diskManager.RotateLeftDiskCounterClockwise();
        }
    }

    public void RotateRightDiskClockwise()
    {
        if (diskManager != null)
        {
            diskManager.RotateRightDiskClockwise();
        }
    }

    public void RotateRightDiskCounterClockwise()
    {
        if (diskManager != null)
        {
            diskManager.RotateRightDiskCounterClockwise();
        }
    }

    // Scene management
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}