// Updated GameManager Script
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
    public Difficulty currentDifficulty;

    public enum Operation { Addition, Subtraction, Multiplication, Division }
    public Operation currentOperation;

    public AudioSource correctSound;
    public AudioSource incorrectSound;

    public Image operatorImage;

    public Sprite plusSprite;
    public Sprite minusSprite;
    public Sprite multiplySprite;
    public Sprite divideSprite;

    private Dictionary<string, Sprite> operatorSprites;

    // Dictionary for disk number arrays
    private Dictionary<string, int[][]> diskNumbersMap;

    // In GameManager.Awake() after setting Instance
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find UI references in the shared HUD
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform hud = canvas.transform.Find("HUD");
            if (hud != null)
            {
                targetNumberText = hud.Find("target_number").GetComponent<TextMeshProUGUI>();
                scoreText = hud.Find("score").GetComponent<TextMeshProUGUI>();
                selectedNumbersText = hud.Find("selected_number").GetComponent<TextMeshProUGUI>();
                operatorImage = hud.Find("OperatorImage").GetComponent<Image>();
            }
        }

        InitializeDiskNumbers();
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

    public void SetOperation(Operation operation)
    {
        currentOperation = operation;

        // Update the operator symbol
        switch (operation)
        {
            case Operation.Addition: currentOperator = "+"; break;
            case Operation.Subtraction: currentOperator = "-"; break;
            case Operation.Multiplication: currentOperator = "*"; break;
            case Operation.Division: currentOperator = "/"; break;
        }

        UpdateOperatorImage();
        GenerateTargetNumber();
    }

    private void InitializeDiskNumbers()
    {
        diskNumbersMap = new Dictionary<string, int[][]>();

        // Addition Easy (sums up to 12) 
        diskNumbersMap["addition_easy"] = new int[][]
        {
            new int[] { 1, 2, 3, 4, 5, 6 },       // Left disk 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk 
        };

        // Addition Medium (sums up to 20) 
        diskNumbersMap["addition_medium"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },      // Left disk 
            new int[] { 5, 6, 7, 8, 9, 10 }       // Right disk 
        };

        // Addition Hard (sums up to 30) 
        diskNumbersMap["addition_hard"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 }, // Left disk 
            new int[] { 5, 7, 9, 10, 11, 13 }     // Right disk 
        };

        // Subtraction Easy (always ensures left > right) 
        diskNumbersMap["subtraction_easy"] = new int[][]
        {
            new int[] { 6, 7, 8, 9, 10, 12 },     // Left disk (larger numbers) 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk (smaller numbers) 
        };

        // Subtraction Medium 
        diskNumbersMap["subtraction_medium"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 }, // Left disk 
            new int[] { 2, 4, 6, 8, 10, 12 }      // Right disk 
        };

        // Subtraction Hard 
        diskNumbersMap["subtraction_hard"] = new int[][]
        {
            new int[] { 15, 18, 20, 25, 30, 35 }, // Left disk 
            new int[] { 5, 8, 10, 12, 15, 18 }    // Right disk 
        };

        // Multiplication Easy (simple multiples up to 12) 
        diskNumbersMap["multiplication_easy"] = new int[][]
        {
            new int[] { 1, 2, 3, 4, 5, 6 },       // Left disk 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk 
        };

        // Multiplication Medium 
        diskNumbersMap["multiplication_medium"] = new int[][]
        {
            new int[] { 2, 3, 4, 5, 6, 7 },       // Left disk 
            new int[] { 2, 3, 4, 5, 6, 7 }        // Right disk 
        };

        // Multiplication Hard 
        diskNumbersMap["multiplication_hard"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },      // Left disk 
            new int[] { 3, 4, 5, 6, 7, 8 }        // Right disk 
        };

        // Division Easy (perfect division, all results are whole numbers) 
        diskNumbersMap["division_easy"] = new int[][]
        {
            new int[] { 2, 4, 6, 8, 10, 12 },     // Left disk (dividend) 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk (divisor) 
        };

        // Division Medium (perfect division) 
        diskNumbersMap["division_medium"] = new int[][]
        {
            new int[] { 10, 15, 20, 25, 30, 35 }, // Left disk 
            new int[] { 2, 5, 5, 5, 5, 5 }        // Right disk 
        };

        // Division Hard (perfect division) 
        diskNumbersMap["division_hard"] = new int[][]
        {
            new int[] { 12, 16, 20, 24, 30, 36 }, // Left disk 
            new int[] { 2, 4, 5, 6, 6, 9 }        // Right disk 
        };
    }

    public void GenerateTargetNumber()
    {
        // Get the appropriate disk arrays based on current operation and difficulty
        string mapKey = currentOperation.ToString().ToLower() + "_" + currentDifficulty.ToString().ToLower();
        int[][] diskNumbers;

        // Check if this operation/difficulty combination exists in our map
        if (!diskNumbersMap.TryGetValue(mapKey, out diskNumbers))
        {
            Debug.LogWarning("Disk numbers not found for key: " + mapKey);
            // Use a fallback option
            targetNumber = Random.Range(1, 20);
            targetNumberText.text = "Target: " + targetNumber;
            return;
        }

        int[] leftDiskNumbers = diskNumbers[0];
        int[] rightDiskNumbers = diskNumbers[1];

        // Generate a valid target number based on the available disk values
        switch (currentOperation)
        {
            case Operation.Addition:
                // Pick random numbers from each disk and set target to their sum
                int leftAddend = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                int rightAddend = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                targetNumber = leftAddend + rightAddend;
                break;

            case Operation.Subtraction:
                // Pick random numbers from each disk and set target to their difference
                int minuend = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                int subtrahend = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                targetNumber = minuend - subtrahend;
                break;

            case Operation.Multiplication:
                // Pick random numbers from each disk and set target to their product
                int multiplicand = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                int multiplier = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                targetNumber = multiplicand * multiplier;
                break;

            case Operation.Division:
                // For division, we need to ensure the result is a whole number
                int dividendIndex = Random.Range(0, leftDiskNumbers.Length);
                int divisorIndex = Random.Range(0, rightDiskNumbers.Length);
                int dividend = leftDiskNumbers[dividendIndex];
                int divisor = rightDiskNumbers[divisorIndex];

                // Ensure we don't divide by zero
                if (divisor == 0)
                {
                    divisor = 1;
                }

                // Check if division results in a whole number
                if (dividend % divisor == 0)
                {
                    targetNumber = dividend / divisor;
                }
                else
                {
                    // If not a whole number, try to find a valid pair
                    bool foundValid = false;
                    for (int i = 0; i < leftDiskNumbers.Length && !foundValid; i++)
                    {
                        for (int j = 0; j < rightDiskNumbers.Length && !foundValid; j++)
                        {
                            int newDividend = leftDiskNumbers[i];
                            int newDivisor = rightDiskNumbers[j];

                            if (newDivisor != 0 && newDividend % newDivisor == 0)
                            {
                                dividend = newDividend;
                                divisor = newDivisor;
                                foundValid = true;
                            }
                        }
                    }

                    targetNumber = dividend / divisor;
                }
                break;
        }

        targetNumberText.text = "Target: " + targetNumber;
        UpdateOperatorImage();
        UpdateSelectedNumbersUI();

        Debug.Log("Generated target number: " + targetNumber + " for " + currentOperation + " " + currentDifficulty);
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

    public bool CheckAnswer()
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

            // Play correct sound
            if (correctSound != null)
                correctSound.Play();

            // Generate a new target number
            GenerateTargetNumber();
        }
        else
        {
            // Play incorrect sound
            if (incorrectSound != null)
                incorrectSound.Play();
        }

        // Return whether the answer was correct for UI feedback
        return isCorrect;
    }
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    #endregion

    // Provide access to disk number arrays for other scripts
    public int[] GetDiskNumbers(bool isLeftDisk)
    {
        string mapKey = currentOperation.ToString().ToLower() + "_" + currentDifficulty.ToString().ToLower();

        if (diskNumbersMap.TryGetValue(mapKey, out int[][] diskNumbers))
        {
            return isLeftDisk ? diskNumbers[0] : diskNumbers[1];
        }

        // Fallback default numbers if not found
        return isLeftDisk ? new int[] { 1, 2, 3, 4, 5, 6 } : new int[] { 1, 2, 3, 4, 5, 6 };
    }
}