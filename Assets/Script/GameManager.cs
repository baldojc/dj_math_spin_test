using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI selectedNumberText;
    public TextMeshProUGUI scoreText;
    public Button checkAnswerButton;
    public Image operatorImage;

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

    void Start()
    {
        checkAnswerButton.onClick.AddListener(CheckAnswer);

        operatorSprites = new Dictionary<string, Sprite>()
        {
            { "+", plusSprite },
            { "-", minusSprite },
            { "*", multiplySprite },
            { "/", divideSprite }
        };

        GenerateTargetNumber();
    }

    public void GenerateTargetNumber()
    {
        targetNumber = Random.Range(2, 20);
        targetNumberText.text = "Target: " + targetNumber.ToString();

        string[] operators = { "+", "-", "*", "/" };
        currentOperator = operators[Random.Range(0, operators.Length)];

        if (operatorSprites.TryGetValue(currentOperator, out Sprite opSprite))
        {
            operatorImage.sprite = opSprite;
        }

        UpdateUI();
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
        selectedNumberText.text = $"Left: {LeftSelectedNumber}, Right: {RightSelectedNumber}";
    }

    void CheckAnswer()
    {
        float result = 0f;
        bool valid = true;

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
                    result = (float)LeftSelectedNumber / RightSelectedNumber;
                else
                    valid = false;
                break;
        }

        if (!valid)
        {
            UIManager.Instance.ShowFeedback(false);
            return;
        }

        if (Mathf.Approximately(result, targetNumber))
        {
            score += 10;
            scoreText.text = "Score: " + score;
            UIManager.Instance.ShowFeedback(true);
            GenerateTargetNumber();
        }
        else
        {
            UIManager.Instance.ShowFeedback(false);
        }
    }
}
