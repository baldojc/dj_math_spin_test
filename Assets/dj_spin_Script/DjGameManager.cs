using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DjGameManager : MonoBehaviour
{
    public static DjGameManager Instance;

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
    public float gameTime = 60f;
    private float currentTime;
    public bool timerActive = false;

    private Dictionary<string, Sprite> operatorSprites;

    private Dictionary<string, int[][]> diskNumbersMap;
    private List<int> recentTargetNumbers = new List<int>();
    private const int MAX_RECENT_TARGETS = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        operatorSprites = new Dictionary<string, Sprite>()
        {
            { "+", plusSprite },
            { "-", minusSprite },
            { "*", multiplySprite },
            { "/", divideSprite }
        };

   
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

    /// <summary>
    /// Central method to reset all state for a new game.
    /// </summary>
    public void ResetGameState(Difficulty difficulty, Operation operation)
    {
        currentDifficulty = difficulty;
        currentOperation = operation;

        score = 0;
        currentStreak = 0;

        switch (operation)
        {
            case Operation.Addition: currentOperator = "+"; break;
            case Operation.Subtraction: currentOperator = "-"; break;
            case Operation.Multiplication: currentOperator = "*"; break;
            case Operation.Division: currentOperator = "/"; break;
        }

        UpdateOperatorImage();
        UpdateScoreUI();
        ClearRecentTargets();
        GenerateTargetNumber();
        ResetTimer();
    }


    public void SetDifficulty(Difficulty difficulty)
    {
        ResetGameState(difficulty, currentOperation);
    }

    public void SetOperation(Operation operation)
    {
        ResetGameState(currentDifficulty, operation);
    }

    private void InitializeDiskNumbers()
    {
        diskNumbersMap = new Dictionary<string, int[][]>();

        diskNumbersMap["addition_easy"] = new int[][]
        {
            new int[] { 1, 2, 3, 4, 5, 6 },
            new int[] { 1, 2, 3, 4, 5, 6 }
        };
        diskNumbersMap["addition_medium"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },
            new int[] { 5, 6, 7, 8, 9, 10 }
        };
        diskNumbersMap["addition_hard"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 },
            new int[] { 5, 7, 9, 10, 11, 13 }
        };
        diskNumbersMap["subtraction_easy"] = new int[][]
        {
            new int[] { 6, 7, 8, 9, 10, 12 },
            new int[] { 1, 2, 3, 4, 5, 6 }
        };
        diskNumbersMap["subtraction_medium"] = new int[][]
        {
            new int[] { 10, 12, 14, 16, 18, 20 },
            new int[] { 2, 4, 6, 8, 10, 12 }
        };
        diskNumbersMap["subtraction_hard"] = new int[][]
        {
            new int[] { 15, 18, 20, 25, 30, 35 },
            new int[] { 5, 8, 10, 12, 15, 18 }
        };
        diskNumbersMap["multiplication_easy"] = new int[][]
        {
            new int[] { 1, 2, 3, 4, 5, 6 },
            new int[] { 1, 2, 3, 4, 5, 6 }
        };
        diskNumbersMap["multiplication_medium"] = new int[][]
        {
            new int[] { 2, 3, 4, 5, 6, 7 },
            new int[] { 2, 3, 4, 5, 6, 7 }
        };
        diskNumbersMap["multiplication_hard"] = new int[][]
        {
            new int[] { 5, 6, 7, 8, 9, 10 },
            new int[] { 3, 4, 5, 6, 7, 8 }
        };
        diskNumbersMap["division_easy"] = new int[][]
        {
            new int[] { 2, 4, 6, 8, 10, 12 },
            new int[] { 1, 2, 3, 4, 5, 6 }
        };
        diskNumbersMap["division_medium"] = new int[][]
        {
            new int[] { 10, 15, 18, 20, 24, 30 },
            new int[] { 2, 3, 3, 4, 6, 5 }
        };
        diskNumbersMap["division_hard"] = new int[][]
        {
            new int[] { 12, 16, 20, 24, 28, 36 },
            new int[] { 2, 4, 5, 6, 7, 9 }
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
        string mapKey = currentOperation.ToString().ToLower() + "_" + currentDifficulty.ToString().ToLower();
        int[][] diskNumbers;

        if (!diskNumbersMap.TryGetValue(mapKey, out diskNumbers))
        {
            Debug.LogWarning("Disk numbers not found for key: " + mapKey);
            targetNumber = Random.Range(1, 20);
            if (targetNumberText != null) targetNumberText.text = "Target " + targetNumber;
            return;
        }

        int[] leftDiskNumbers = diskNumbers[0];
        int[] rightDiskNumbers = diskNumbers[1];

        int attempts = 0;
        int maxAttempts = 10;
        bool foundValidTarget = false;

        List<int> allPossibleResults = GetAllPossibleResults();

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

            if (validTargets.Count > 0)
            {
                int randomIndex = Random.Range(0, validTargets.Count);
                targetNumber = validTargets[randomIndex];
                foundValidTarget = true;
            }
            else
            {
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

        if (targetNumberText != null) targetNumberText.text = "Target " + targetNumber;
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

            if (streakBonus > 0)
            {
                Debug.Log($"Streak: {currentStreak}! Bonus points: +{streakBonus}");
            }

            DjAudioManager.Instance.PlaySound("Correct");
            DjAudioManager.Instance.PlaySound("Correct", pitch: Random.Range(0.95f, 1.05f) + (streakBonus * 0.02f));

            GenerateTargetNumber();
        }
        else
        {
            currentStreak = 0;

            DjAudioManager.Instance.PlaySound("Incorrect");
            DjAudioManager.Instance.PlaySound("Scratch", pitch: 0.8f);
        }

        return isCorrect;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
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
        Debug.Log("Timer reset to " + gameTime + " seconds (ACTIVE: " + timerActive + ")");
    }
    public void PauseTimer()
    {
        timerActive = false;
        Debug.Log("Timer paused (ACTIVE: " + timerActive + ")");
    }
    public void ResumeTimer()
    {
        timerActive = true;
        Debug.Log("Timer resumed (ACTIVE: " + timerActive + ")");
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

        if (DjUIManager.Instance != null)
        {
            DjUIManager.Instance.ShowGameOver(score, highScore);
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