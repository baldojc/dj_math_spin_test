using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject mainMenuPanel;
    public GameObject difficultyMenuPanel;
    public GameObject gamePanel;
    public GameObject pausePanel;
    public GameObject feedbackPanel;
    public Image feedbackImage;
    public Sprite correctSprite;
    public Sprite incorrectSprite;

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
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        difficultyMenuPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void ShowDifficultyMenu()
    {
        mainMenuPanel.SetActive(false);
        difficultyMenuPanel.SetActive(true);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        difficultyMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);

        GameManager.Instance.GenerateTargetNumber();
    }

    public void TogglePause()
    {
        bool isPaused = pausePanel.activeSelf;
        pausePanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1 : 0;
    }

    public void ShowFeedback(bool isCorrect)
    {
        feedbackPanel.SetActive(true);
        feedbackImage.sprite = isCorrect ? correctSprite : incorrectSprite;
        CancelInvoke("HideFeedback");
        Invoke("HideFeedback", 1.5f);
    }

  
}
