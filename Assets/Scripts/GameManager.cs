using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI selectedNumberText;
    public TextMeshProUGUI operatorText;
    public TextMeshProUGUI scoreText;
    public Button checkAnswerButton;

    private int targetNumber;
    private int score = 0;
    private string currentOperator;

    public int LeftSelectedNumber { get; private set; } = 3;
    public int RightSelectedNumber { get; private set; } = 1;

    void Start()
    {
        GenerateTargetNumber();
        checkAnswerButton.onClick.AddListener(CheckAnswer);
    }

    void GenerateTargetNumber()
    {
        targetNumber = Random.Range(1, 7);
        targetNumberText.text = "Target: " + targetNumber.ToString();

        string[] operators = { "+", "-", "*", "/" };
        currentOperator = operators[Random.Range(0, operators.Length)];
        operatorText.text = currentOperator;
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
