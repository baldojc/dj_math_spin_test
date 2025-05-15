using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI selectedNumbersText;

    public int targetNumber;
    public int score = 0;
    private int currentStreak = 0;
    private const int MAX_STREAK_BONUS = 5;

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

    public TextMeshProUGUI timerText;
    public float gameTime = 60f; // 60 seconds by default
    private float currentTime;
    public bool timerActive = false;

    private Dictionary<string, Sprite> operatorSprites;

    // Dictionary for disk number arrays
    private Dictionary<string, int[][]> diskNumbersMap;

    // Target number history to prevent immediate repetition
    private List<int> recentTargetNumbers = new List<int>();
    private const int MAX_RECENT_TARGETS = 5;

    // In GameManager.Awake() after setting Instance
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Add this line
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find UI references in the shared HUD more reliably
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Transform hud = canvas.transform.Find("HUD");
            if (hud != null)
            {
                Debug.Log("Found HUD in canvas: " + canvas.name);
                targetNumberText = hud.Find("target_number")?.GetComponent<TextMeshProUGUI>();
                scoreText = hud.Find("score")?.GetComponent<TextMeshProUGUI>();
                selectedNumbersText = hud.Find("selected_number")?.GetComponent<TextMeshProUGUI>();
                operatorImage = hud.Find("OperatorImage")?.GetComponent<Image>();
                timerText = hud.Find("timer")?.GetComponent<TextMeshProUGUI>();

                // Add these checks to ensure references are found
                if (targetNumberText == null) Debug.LogError("target_number not found or missing TextMeshProUGUI component");
                if (scoreText == null) Debug.LogError("score not found or missing TextMeshProUGUI component");
                if (selectedNumbersText == null) Debug.LogError("selected_number not found or missing TextMeshProUGUI component");
                if (operatorImage == null) Debug.LogError("OperatorImage not found or missing Image component");
                if (timerText == null) Debug.LogError("timer not found or missing TextMeshProUGUI component");

                break;
            }
        }

        InitializeDiskNumbers();
    }

    private void Start()
    {
        score = 0;
        currentStreak = 0;
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

        // Initialize the timer
        InitializeTimer();
    }

    private void Update()
    {
        if (timerActive && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }

    #region Game Flow

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        score = 0;
        currentStreak = 0;
        UpdateScoreUI();
        ClearRecentTargets();
        GenerateTargetNumber();
        ResetTimer();
    }

    public void SetOperation(Operation operation)
    {
        currentOperation = operation;

        switch (operation)
        {
            case Operation.Addition: currentOperator = "+"; break;
            case Operation.Subtraction: currentOperator = "-"; break;
            case Operation.Multiplication: currentOperator = "*"; break;
            case Operation.Division: currentOperator = "/"; break;
        }

        UpdateOperatorImage();
        ClearRecentTargets();
        GenerateTargetNumber();
        ResetTimer();
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

        diskNumbersMap["addition_medium"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },      // Left disk 
            new int[] { 5, 6, 7, 8, 9, 10 }       // Right disk 
        };

        diskNumbersMap["addition_hard"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 }, // Left disk 
            new int[] { 5, 7, 9, 10, 11, 13 }     // Right disk 
        };

        diskNumbersMap["subtraction_easy"] = new int[][]
        {
            new int[] { 6, 7, 8, 9, 10, 12 },     // Left disk (larger numbers) 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk (smaller numbers) 
        };

        diskNumbersMap["subtraction_medium"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 }, // Left disk 
            new int[] { 2, 4, 6, 8, 10, 12 }      // Right disk 
        };

        diskNumbersMap["subtraction_hard"] = new int[][]
        {
            new int[] { 15, 18, 20, 25, 30, 35 }, // Left disk 
            new int[] { 5, 8, 10, 12, 15, 18 }    // Right disk 
        };

        diskNumbersMap["multiplication_easy"] = new int[][]
        {
            new int[] { 1, 2, 3, 4, 5, 6 },       // Left disk 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk 
        };

        diskNumbersMap["multiplication_medium"] = new int[][]
        {
            new int[] { 2, 3, 4, 5, 6, 7 },       // Left disk 
            new int[] { 2, 3, 4, 5, 6, 7 }        // Right disk 
        };

        diskNumbersMap["multiplication_hard"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },      // Left disk 
            new int[] { 3, 4, 5, 6, 7, 8 }        // Right disk 
        };


        diskNumbersMap["division_easy"] = new int[][]
        {
            new int[] { 2, 4, 6, 8, 10, 12 },     // Left disk (dividend) 
            new int[] { 1, 2, 3, 4, 5, 6 }        // Right disk (divisor) 
        };

        // Division Medium (perfect division) - Now more varied
        diskNumbersMap["division_medium"] = new int[][]
        {
            new int[] { 10, 15, 18, 20, 24, 30 }, // Left disk 
            new int[] { 2, 3, 3, 4, 6, 5 }        // Right disk 
        };

        // Division Hard (perfect division) - Now more varied
        diskNumbersMap["division_hard"] = new int[][]
        {
            new int[] { 12, 16, 20, 24, 28, 36 }, // Left disk 
            new int[] { 2, 4, 5, 6, 7, 9 }        // Right disk 
        };
    }

    private void ClearRecentTargets()
    {
        recentTargetNumbers.Clear();
    }

    private bool IsTargetRepeated(int target)
    {
        return recentTargetNumbers.Contains(target);
    }

    private void AddToRecentTargets(int target)
    {
        recentTargetNumbers.Add(target);
        if (recentTargetNumbers.Count > MAX_RECENT_TARGETS)
        {
            recentTargetNumbers.RemoveAt(0);
        }
    }

    // Method to find all possible results from the current disks
    private List<int> GetAllPossibleResults()
    {
        string mapKey = currentOperation.ToString().ToLower() + "_" + currentDifficulty.ToString().ToLower();
        if (!diskNumbersMap.TryGetValue(mapKey, out int[][] diskNumbers))
        {
            return new List<int>();
        }

        int[] leftDiskNumbers = diskNumbers[0];
        int[] rightDiskNumbers = diskNumbers[1];
        List<int> possibleResults = new List<int>();

        // Calculate all possible results based on operation
        foreach (int left in leftDiskNumbers)
        {
            foreach (int right in rightDiskNumbers)
            {
                int result = 0;
                bool validResult = true;

                switch (currentOperation)
                {
                    case Operation.Addition:
                        result = left + right;
                        break;
                    case Operation.Subtraction:
                        result = left - right;
                        break;
                    case Operation.Multiplication:
                        result = left * right;
                        break;
                    case Operation.Division:
                        if (right != 0 && left % right == 0)
                        {
                            result = left / right;
                        }
                        else
                        {
                            validResult = false;
                        }
                        break;
                }

                if (validResult && !possibleResults.Contains(result))
                {
                    possibleResults.Add(result);
                }
            }
        }

        return possibleResults;
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
            targetNumberText.text = "Target " + targetNumber;
            return;
        }

        int[] leftDiskNumbers = diskNumbers[0];
        int[] rightDiskNumbers = diskNumbers[1];

        // Try to find a non-repeated target up to 10 attempts
        int attempts = 0;
        int maxAttempts = 10;
        bool foundValidTarget = false;

        // Get all possible targets for the current setup
        List<int> allPossibleResults = GetAllPossibleResults();

        // Remove recent targets from possibilities
        List<int> validTargets = new List<int>(allPossibleResults);
        foreach (int recent in recentTargetNumbers)
        {
            validTargets.Remove(recent);
        }

        if (validTargets.Count == 0)
        {
            validTargets = new List<int>(allPossibleResults);
        }

        while (attempts < maxAttempts && !foundValidTarget)
        {
            attempts++;

            // Select a random valid target
            if (validTargets.Count > 0)
            {
                int randomIndex = Random.Range(0, validTargets.Count);
                targetNumber = validTargets[randomIndex];
                foundValidTarget = true;
            }
            else
            {
                // Fallback generation logic
                switch (currentOperation)
                {
                    case Operation.Addition:
                        int leftAddend = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                        int rightAddend = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                        targetNumber = leftAddend + rightAddend;
                        break;

                    case Operation.Subtraction:
                        int minuend = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                        int subtrahend = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                        targetNumber = minuend - subtrahend;
                        break;

                    case Operation.Multiplication:
                        int multiplicand = leftDiskNumbers[Random.Range(0, leftDiskNumbers.Length)];
                        int multiplier = rightDiskNumbers[Random.Range(0, rightDiskNumbers.Length)];
                        targetNumber = multiplicand * multiplier;
                        break;

                    case Operation.Division:
                        List<KeyValuePair<int, int>> validDivisionPairs = new List<KeyValuePair<int, int>>();

                        for (int i = 0; i < leftDiskNumbers.Length; i++)
                        {
                            for (int j = 0; j < rightDiskNumbers.Length; j++)
                            {
                                int dividend = leftDiskNumbers[i];
                                int divisor = rightDiskNumbers[j];

                                if (divisor != 0 && dividend % divisor == 0)
                                {
                                    validDivisionPairs.Add(new KeyValuePair<int, int>(dividend, divisor));
                                }
                            }
                        }

                        if (validDivisionPairs.Count > 0)
                        {
                            int randomPair = Random.Range(0, validDivisionPairs.Count);
                            int dividend = validDivisionPairs[randomPair].Key;
                            int divisor = validDivisionPairs[randomPair].Value;
                            targetNumber = dividend / divisor;
                        }
                        else
                        {
                            // Default fallback
                            targetNumber = 2;
                        }
                        break;
                }

                if (!IsTargetRepeated(targetNumber))
                {
                    foundValidTarget = true;
                }
            }
        }

        AddToRecentTargets(targetNumber);

        targetNumberText.text = "Target " + targetNumber;
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
            case "+":
                result = LeftSelectedNumber + RightSelectedNumber;
                isCorrect = (result == targetNumber);
                break;
            case "-":
                result = LeftSelectedNumber - RightSelectedNumber;
                isCorrect = (result == targetNumber);
                break;
            case "*":
                result = LeftSelectedNumber * RightSelectedNumber;
                isCorrect = (result == targetNumber);
                break;
            case "/":
                if (RightSelectedNumber != 0)
                {
                    // For division, we only want exact integer divisions
                    if (LeftSelectedNumber % RightSelectedNumber == 0)
                    {
                        result = LeftSelectedNumber / RightSelectedNumber;
                        isCorrect = (result == targetNumber);
                    }
                }
                break;
        }

        if (isCorrect)
        {
            currentStreak++;
            int streakBonus = Mathf.Min(currentStreak - 1, MAX_STREAK_BONUS);
            int pointsToAdd = 10 + streakBonus;

            score += pointsToAdd;
            UpdateScoreUI();

            // Show bonus points effect
            if (streakBonus > 0)
            {
                Debug.Log($"Streak: {currentStreak}! Bonus points: +{streakBonus}");
            }

            AudioManager.Instance.PlaySound("Correct");
            AudioManager.Instance.PlaySound("Correct", pitch: Random.Range(0.95f, 1.05f) + (streakBonus * 0.02f));

            GenerateTargetNumber();
        }
        else
        {
            currentStreak = 0;

            AudioManager.Instance.PlaySound("Incorrect");
            // Optional: Add record scratch sound
            AudioManager.Instance.PlaySound("Scratch", pitch: 0.8f);
        }

        return isCorrect;
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    #endregion

    #region Timer Functions

    public void InitializeTimer()
    {
        if (timerText == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                Transform hud = canvas.transform.Find("HUD");
                if (hud != null)
                {
                    timerText = hud.Find("timer")?.GetComponent<TextMeshProUGUI>();
                    if (timerText == null) Debug.LogError("timer not found or missing TextMeshProUGUI component");
                    break;
                }
            }
        }

        ResetTimer();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(currentTime);
            timerText.text = seconds.ToString();
        }
    }

    public void ResetTimer()
    {
        currentTime = gameTime;
        UpdateTimerUI();
        timerActive = true;
        Debug.Log("Timer reset to " + gameTime + " seconds");
    }

    public void PauseTimer()
    {
        timerActive = false;
        Debug.Log("Timer paused");
    }

    public void ResumeTimer()
    {
        timerActive = true;
        Debug.Log("Timer resumed");
    }

    private void GameOver()
    {
        timerActive = false;
        Debug.Log("Game over! Final score: " + score);

        string highScoreKey = $"HighScore_{currentOperation}_{currentDifficulty}";

        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt(highScoreKey, score);
            PlayerPrefs.Save();
            Debug.Log($"New high score for {currentOperation} {currentDifficulty}: {score}");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(score, highScore);
        }
    }

    public int GetHighScore(Operation operation, Difficulty difficulty)
    {
        string highScoreKey = $"HighScore_{operation}_{difficulty}";
        return PlayerPrefs.GetInt(highScoreKey, 0);
    }

    #endregion

    public int[] GetDiskNumbers(bool isLeftDisk)
    {
        string mapKey = $"{currentOperation.ToString().ToLower()}_{currentDifficulty.ToString().ToLower()}";

        if (diskNumbersMap.TryGetValue(mapKey, out int[][] diskNumbers))
        {
            return isLeftDisk ? diskNumbers[0] : diskNumbers[1];
        }

        return new int[] { 1, 2, 3, 4, 5, 6 };
    }
}