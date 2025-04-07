using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public Image operatorImage;

    // Dictionary for dynamic sprite loading
    private Dictionary<string, Sprite> operatorSprites = new Dictionary<string, Sprite>();

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
        LoadOperatorSprites();
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
                targetNumber = Random.Range(3, 18);
                break;
            case Difficulty.Medium:
                targetNumber = Random.Range(1, 100);
                break;
            case Difficulty.Hard:
                targetNumber = Random.Range(1, 200);
                break;
        }

        targetNumberText.text = "Target: " + targetNumber;

        // Randomly select operator
        string[] operators = { "+", "-", "*", "/" };
        currentOperator = operators[Random.Range(0, operators.Length)];

        UpdateOperatorImage();
        UpdateSelectedNumbersUI();
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

    private void LoadOperatorSprites()
    {
        operatorSprites["+"] = Resources.Load<Sprite>("Sprite/plus");
        operatorSprites["-"] = Resources.Load<Sprite>("Sprite/minus");
        operatorSprites["*"] = Resources.Load<Sprite>("Sprite/multiply");
        operatorSprites["/"] = Resources.Load<Sprite>("Sprite/divide");
    }

    private void UpdateOperatorImage()
    {
        if (operatorImage == null) return;

        if (operatorSprites.TryGetValue(currentOperator, out Sprite opSprite))
        {
            operatorImage.sprite = opSprite;
        }
        else
        {
            Debug.LogWarning("Operator sprite not found for: " + currentOperator);
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
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    #endregion
}
