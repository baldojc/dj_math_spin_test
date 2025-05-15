using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DjUIManager : MonoBehaviour
{
    public static DjUIManager Instance;

 
    public GameObject mainMenuPanel;
    public GameObject operationPickingPanel;
    public GameObject difficultyMenuPanel;
    public GameObject gameOverPanel;
    public GameObject howToPlayPanel;
    public GameObject loadingScreenPanel;

    public GameObject gamePanel;

  
    public GameObject pausePanel;
    public GameObject feedbackPanel;

  
    private LoadingScreenManager loadingScreenManager;

 
    public Sprite correctSprite;
    public Sprite incorrectSprite;

 
    private GameObject currentGamePanel;


    private string currentOperationStr;
    private string currentDifficultyStr;

  
    private bool isGamePaused = false;

 
    public Button mainMenuMusicButton;
    public Button mainMenuMuteButton;

   
    public Button pauseMusicButton;
    public Button pauseMuteButton;


    private bool isInGameplay = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingScreenPanel == null)
        {
     
            loadingScreenPanel = GameObject.Find("LoadingScreenPanel");

            if (loadingScreenPanel == null)
            {
                Debug.LogError("LoadingScreenPanel is missing! Creating a temporary panel.");
           
                loadingScreenPanel = new GameObject("LoadingScreenPanel");
                loadingScreenPanel.transform.SetParent(transform, false);
            }
        }

      
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

   
    private void FindAllAudioButtons()
    {
    
        if (mainMenuPanel != null)
        {
            mainMenuMusicButton = mainMenuPanel.transform.Find("music_button")?.GetComponent<Button>();
            mainMenuMuteButton = mainMenuPanel.transform.Find("mute_button")?.GetComponent<Button>();
        }

     
        if (pausePanel != null)
        {
            pauseMusicButton = pausePanel.transform.Find("music_button")?.GetComponent<Button>();
            pauseMuteButton = pausePanel.transform.Find("mute_button")?.GetComponent<Button>();
        }
    }

   
    private void SetupAllAudioButtons()
    {
      
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

      
        if (pauseMusicButton != null)
        {
            pauseMusicButton.onClick.RemoveAllListeners();
            pauseMusicButton.onClick.AddListener(OnPauseMusicButtonClicked);  
        }

        if (pauseMuteButton != null)
        {
            pauseMuteButton.onClick.RemoveAllListeners();
            pauseMuteButton.onClick.AddListener(OnMuteButtonClicked);
        }
    }

    public void ShowMainMenu()
    {
    
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.PlayMenuMusic();
        }

        HideAllPanels();
        isInGameplay = false;  

        if (DjGameManager.Instance != null)
        {
            DjGameManager.Instance.PauseTimer();
        }

        ToggleHUD(false); 
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
      
        if (string.IsNullOrEmpty(currentOperationStr))
            currentOperationStr = "addition";
        if (string.IsNullOrEmpty(currentDifficultyStr))
            currentDifficultyStr = "easy";

        
        DjGameManager.Operation operation = GetOperationFromString(currentOperationStr);
        DjGameManager.Difficulty difficulty = GetDifficultyFromString(currentDifficultyStr);

     
        if (DjGameManager.Instance != null)
        {
            DjGameManager.Instance.SetOperation(operation);
            DjGameManager.Instance.SetDifficulty(difficulty);
        }

       
        ShowLoadingScreen();
    }

   
    private void ShowLoadingScreen()
    {
        HideAllPanels();

        
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.PlaySound("Button");
        }

      
        if (loadingScreenPanel == null)
        {
            Debug.LogError("Loading screen panel reference lost! Trying to find it.");
            loadingScreenPanel = GameObject.Find("LoadingScreenPanel");

          
            if (loadingScreenPanel == null)
            {
                Debug.LogError("Creating fallback loading screen panel");
                loadingScreenPanel = new GameObject("LoadingScreenPanel");
                loadingScreenPanel.transform.SetParent(transform, false);
                loadingScreenManager = loadingScreenPanel.AddComponent<LoadingScreenManager>();
            }
        }

       
        if (loadingScreenManager == null)
        {
            loadingScreenManager = loadingScreenPanel.GetComponent<LoadingScreenManager>();

            if (loadingScreenManager == null)
            {
                Debug.LogError("LoadingScreenManager component missing, adding it now");
                loadingScreenManager = loadingScreenPanel.AddComponent<LoadingScreenManager>();
            }
        }

     
        loadingScreenPanel.SetActive(true);

        Debug.Log("Starting loading screen coroutine");

  
        StartCoroutine(loadingScreenManager.ShowLoadingScreen(StartGame));
    }

    private DjGameManager.Operation GetOperationFromString(string operationStr)
    {
        switch (operationStr.ToLower())
        {
            case "addition": return DjGameManager.Operation.Addition;
            case "subtraction": return DjGameManager.Operation.Subtraction;
            case "multiplication": return DjGameManager.Operation.Multiplication;
            case "division": return DjGameManager.Operation.Division;
            default: return DjGameManager.Operation.Addition;
        }
    }

    private DjGameManager.Difficulty GetDifficultyFromString(string difficultyStr)
    {
        switch (difficultyStr.ToLower())
        {
            case "easy": return DjGameManager.Difficulty.Easy;
            case "medium": return DjGameManager.Difficulty.Medium;
            case "hard": return DjGameManager.Difficulty.Hard;
            default: return DjGameManager.Difficulty.Easy;
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game after loading screen");

        isInGameplay = true; 

     
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.PlayGameplayMusic();
        }

        HideAllPanels();
        ToggleHUD(true);

        gamePanel.SetActive(true);
        SetupDisksInCurrentPanel();

        if (DjGameManager.Instance != null)
        {
            DjGameManager.Instance.GenerateTargetNumber();
            DjGameManager.Instance.ResetTimer();
            DjGameManager.Instance.ResumeTimer();
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
         
            if (DjAudioManager.Instance != null)
            {
                DjAudioManager.Instance.PauseMusic();
            }

            if (DjGameManager.Instance != null)
            {
                DjGameManager.Instance.PauseTimer();
            }
        }
        else
        {
            
            if (DjAudioManager.Instance != null)
            {
                DjAudioManager.Instance.ResumeMusic();
            }

            if (DjGameManager.Instance != null)
            {
                DjGameManager.Instance.ResumeTimer();
            }
        }
    }

    public void ShowFeedback(bool isCorrect)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);

           
            Transform correctImage = feedbackPanel.transform.Find("correct");
            Transform incorrectImage = feedbackPanel.transform.Find("incorrect");

            if (isCorrect)
            {
                correctImage.gameObject.SetActive(true);
                incorrectImage.gameObject.SetActive(false);

                if (DjAudioManager.Instance != null)
                {
                    DjAudioManager.Instance.PlaySound("Correct");
                    DjAudioManager.Instance.PlaySound("Cheer", pitch: Random.Range(0.95f, 1.05f));
                }
            }
            else
            {
                correctImage.gameObject.SetActive(false);
                incorrectImage.gameObject.SetActive(true);

                if (DjAudioManager.Instance != null)
                {
                    DjAudioManager.Instance.PlaySound("Incorrect");
                    DjAudioManager.Instance.PlaySound("Scratch", pitch: 0.8f);
                }
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
            isInGameplay = false; 

          
            TMPro.TextMeshProUGUI finalScoreText = gameOverPanel.transform.Find("final_score")?.GetComponent<TMPro.TextMeshProUGUI>();
            TMPro.TextMeshProUGUI highScoreText = gameOverPanel.transform.Find("high_score")?.GetComponent<TMPro.TextMeshProUGUI>();

            if (finalScoreText != null)
                finalScoreText.text = "Score: " + finalScore;

            if (highScoreText != null && DjGameManager.Instance != null)
            {
                
                string operationName = DjGameManager.Instance.currentOperation.ToString();
                string difficultyName = DjGameManager.Instance.currentDifficulty.ToString();
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
        if (DjGameManager.Instance != null)
        {
            
            bool wasCorrect = DjGameManager.Instance.CheckAnswer();

           
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
        if (DjGameManager.Instance != null)
        {
            DjGameManager.Instance.PauseTimer();
        }

        
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.StopMusic();
        }

        ShowMainMenu();
    }

    public void OnRestartButtonClicked()
    {
        
        if (pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
            isGamePaused = false;
        }

     
        if (string.IsNullOrEmpty(currentOperationStr))
            currentOperationStr = "addition";
        if (string.IsNullOrEmpty(currentDifficultyStr))
            currentDifficultyStr = "easy";

        
        ShowLoadingScreen();
    }

    public void ToggleHUD(bool show)
    {
       
        Canvas mainCanvas = FindAnyObjectByType<Canvas>();
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

    
    public void OnMusicButtonClicked()
    {
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.EnableMusic();
           
            DjAudioManager.Instance.PlayMenuMusic();
        }
    }

    
    public void OnPauseMusicButtonClicked()
    {
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.EnableMusic();
         
            if (isInGameplay)
            {
                DjAudioManager.Instance.PlayGameplayMusic();
            }
            else
            {
                DjAudioManager.Instance.PlayMenuMusic();
            }
        }
    }

    public void OnMuteButtonClicked()
    {
        if (DjAudioManager.Instance != null)
        {
            DjAudioManager.Instance.DisableMusic();
        }
    }
}