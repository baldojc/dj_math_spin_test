using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI selectedNumbersText;

    public int targetNumber;
    public int score = 0;

    public int LeftSelectedNumber;
    public int RightSelectedNumber;
    public string currentOperator = "+";

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty = Difficulty.Easy;

    public AudioSource correctSound;
    public AudioSource incorrectSound;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        score = 0;
        UpdateScoreUI();
    }

    #region Game Flow

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        score = 0;
        UpdateScoreUI();
        GenerateTargetNumber();
    }

    public void GenerateTargetNumber()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                targetNumber = Random.Range(5, 21); // Simple numbers
                break;
            case Difficulty.Medium:
                targetNumber = Random.Range(10, 51);
                break;
            case Difficulty.Hard:
                targetNumber = Random.Range(20, 100);
                break;
        }

        targetNumberText.text = "Target: " + targetNumber;
    }


    public void UpdateLeftNumber(int number)
    {
        LeftSelectedNumber = number;
        UpdateSelectedNumbersUI();
    }

    public void UpdateRightNumber(int number)
    {
        RightSelectedNumber = number;
        UpdateSelectedNumbersUI();
    }

    private void UpdateSelectedNumbersUI()
    {
        if (selectedNumbersText != null)
        {
            selectedNumbersText.text = $"{LeftSelectedNumber} {currentOperator} {RightSelectedNumber}";
        }
    }

    public void CheckAnswer()
    {
        int result = 0;
        bool isCorrect = false;

        switch (currentOperator)
        {
            case "+": result = LeftSelectedNumber + RightSelectedNumber; break;
            case "-": result = LeftSelectedNumber - RightSelectedNumber; break;
            case "*": result = LeftSelectedNumber * RightSelectedNumber; break;
            case "/":
                if (RightSelectedNumber != 0)
                    result = LeftSelectedNumber / RightSelectedNumber;
                break;
        }

        if (result == targetNumber)
        {
            isCorrect = true;
            score += 10;
            UpdateScoreUI();
            GenerateTargetNumber();

            if (correctSound != null)
                correctSound.Play();
        }
        else
        {
            if (incorrectSound != null)
                incorrectSound.Play();
        }

        UIManager.Instance.ShowFeedback(isCorrect);
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    #endregion
}
