using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string currentOperator; // "+" for addition, "-" for subtraction, etc.
    public int targetNumber;
    public int[] numbers;

    // Store the left and right numbers (for the two disks)
    public int leftNumber;
    public int rightNumber;

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty;

    // References to the TextMesh Pro components
    public TextMeshProUGUI leftNumberText;
    public TextMeshProUGUI rightNumberText;
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI operatorText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetOperator(string operatorChoice)
    {
        currentOperator = operatorChoice;
        UpdateOperatorImage();
    }

    public void GenerateTargetNumber()
    {
        // Generate target number based on the selected operator
        switch (currentOperator)
        {
            case "+":
                targetNumber = Random.Range(1, 10) + Random.Range(1, 10); // Example for easy level
                break;
            case "-":
                targetNumber = Random.Range(10, 20) - Random.Range(1, 10); // Example for easy level
                break;
            case "*":
                targetNumber = Random.Range(1, 5) * Random.Range(1, 5); // Example for easy level
                break;
            case "/":
                targetNumber = Random.Range(10, 20); // Just a placeholder
                break;
        }

        // Set the numbers for the disks
        SetNumbersForDifficulty();

        // Update the UI texts
        targetNumberText.text = "Target: " + targetNumber.ToString();
        operatorText.text = currentOperator;
        UpdateLeftNumber(leftNumber);
        UpdateRightNumber(rightNumber);
    }

    void SetNumbersForDifficulty()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                numbers = new int[] { 1, 2, 3, 4, 5 };
                break;
            case Difficulty.Medium:
                numbers = new int[] { 6, 7, 8, 9, 10 };
                break;
            case Difficulty.Hard:
                numbers = new int[] { 11, 12, 13, 14, 15 };
                break;
        }
    }

    public void UpdateLeftNumber(int newNumber)
    {
        leftNumber = newNumber;
        leftNumberText.text = newNumber.ToString(); // Update the left number TextMesh Pro
    }

    public void UpdateRightNumber(int newNumber)
    {
        rightNumber = newNumber;
        rightNumberText.text = newNumber.ToString(); // Update the right number TextMesh Pro
    }

    public void UpdateOperatorImage()
    {
        // This will update the operator image based on the selected operator
        UIManager.Instance.ShowFeedback(currentOperator == "+" ? true : false);
    }
}
