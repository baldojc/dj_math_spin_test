using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI selectedNumberText;
    public TextMeshProUGUI scoreText;
    public Button checkAnswerButton;

    public Image operatorImage;

    // Assign these in the Inspector
    public Sprite plusSprite;
    public Sprite minusSprite;
    public Sprite multiplySprite;
    public Sprite divideSprite;

    private Dictionary<string, Sprite> operatorSprites;

    private int targetNumber;
    private int score = 0;
    private string currentOperator;

    public int LeftSelectedNumber { get; private set; } = 3;
    public int RightSelectedNumber { get; private set; } = 1;

    void Start()
    {
        checkAnswerButton.onClick.AddListener(CheckAnswer);

        // Initialize operatorSprites dictionary using manually assigned sprites
        operatorSprites = new Dictionary<string, Sprite>()
        {
            { "+", plusSprite },
            { "-", minusSprite },
            { "*", multiplySprite },
            { "/", divideSprite }
        };

        GenerateTargetNumber();
    }

    void GenerateTargetNumber()
    {
        targetNumber = Random.Range(1, 7);
        targetNumberText.text = "Target: " + targetNumber.ToString();

        string[] operators = { "+", "-", "*", "/" };
        currentOperator = operators[Random.Range(0, operators.Length)];

        if (operatorSprites.TryGetValue(currentOperator, out Sprite opSprite))
        {
            operatorImage.sprite = opSprite;
        }
    }

    public void UpdateLeftNumber(int number)
    {
        LeftSelectedNumber = number;
        UpdateUI();
    }

    public void UpdateRightNumber(int number)
    {
        RightSelectedNumber = number;
        UpdateUI();
    }

    void UpdateUI()
    {
        selectedNumberText.text = "Left: " + LeftSelectedNumber + ", Right: " + RightSelectedNumber;
    }

    void CheckAnswer()
    {
        int result = 0;

        switch (currentOperator)
        {
            case "+":
                result = LeftSelectedNumber + RightSelectedNumber;
                break;
            case "-":
                result = LeftSelectedNumber - RightSelectedNumber;
                break;
            case "*":
                result = LeftSelectedNumber * RightSelectedNumber;
                break;
            case "/":
                if (RightSelectedNumber != 0)
                    result = LeftSelectedNumber / RightSelectedNumber;
                break;
        }

        if (result == targetNumber)
        {
            score += 10;
            scoreText.text = "Score: " + score;
            GenerateTargetNumber();
        }
    }
}
