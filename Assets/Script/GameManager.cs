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

    public Sprite plusSprite;
    public Sprite minusSprite;
    public Sprite multiplySprite;
    public Sprite divideSprite;

    private Dictionary<string, Sprite> operatorSprites;

    // Static flag for addition operator
    private static bool isAdditionOperatorStatic = false;

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

        // Initialize sprite dictionary
        operatorSprites = new Dictionary<string, Sprite>()
        {
            { "+", plusSprite },
            { "-", minusSprite },
            { "*", multiplySprite },
            { "/", divideSprite }
        };

        // Initial game setup
        GenerateTargetNumber();
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
        // Set target number based on difficulty
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

        string[] operators = { "+" };
        currentOperator = operators[Random.Range(0, operators.Length)];

        if (currentOperator == "+")
        {
            isAdditionOperatorStatic = true;
        }
        else
        {
            isAdditionOperatorStatic = false;
        }

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

    private void UpdateOperatorImage()
    {
        if (operatorImage != null && operatorSprites != null && operatorSprites.TryGetValue(currentOperator, out Sprite sprite))
        {
            operatorImage.sprite = sprite;
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

    // Static access to check if the operator is addition
    public static bool IsAdditionOperatorStatic()
    {
        return isAdditionOperatorStatic;
    }
}
