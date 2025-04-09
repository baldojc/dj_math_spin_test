using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Operation types
    public enum Operation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    // Difficulty levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    // References
    [Header("Disk References")]
    public GameObject leftDisk;
    public GameObject rightDisk;
    public TextMeshPro[] leftDiskTexts = new TextMeshPro[5];
    public TextMeshPro[] rightDiskTexts = new TextMeshPro[5];
    private DiskRotation leftDiskRotation;
    private DiskRotation rightDiskRotation;

    [Header("UI References")]
    public UI_Manager uiManager;
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI selectedNumberText;
    public Image operatorImage;

    [Header("Operator Sprites")]
    public Sprite additionSprite;
    public Sprite subtractionSprite;
    public Sprite multiplicationSprite;
    public Sprite divisionSprite;

    // Game settings
    [Header("Game Settings")]
    public Operation currentOperation = Operation.Addition;
    public Difficulty currentDifficulty = Difficulty.Easy;

    // Game state
    private int targetNumber;
    private int selectedLeftNumber;
    private int selectedRightNumber;
    private int currentScore = 0;
    private float timeLeft = 60f;

    // Selected indices
    private int selectedLeftIndex = 0;
    private int selectedRightIndex = 0;

    // Constants for number ranges
    private readonly Dictionary<Difficulty, (int min, int max)> difficultyRanges = new Dictionary<Difficulty, (int min, int max)>
    {
        { Difficulty.Easy, (1, 10) },
        { Difficulty.Medium, (5, 20) },
        { Difficulty.Hard, (10, 30) }
    };

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameManager: Start method called");

        // Find references if not set
        FindReferences();

        // Test initialization to ensure disks have numbers right away
        InitializeGame(Operation.Addition, Difficulty.Easy);
    }

    // Find all necessary references
    private void FindReferences()
    {
        // Find UI Manager
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UI_Manager>();
            Debug.Log("GameManager: " + (uiManager != null ? "UI_Manager found" : "UI_Manager NOT found"));
        }

        // Find disks if not assigned
        if (leftDisk == null)
        {
            leftDisk = GameObject.Find("LeftDisk");
            Debug.Log("GameManager: " + (leftDisk != null ? "LeftDisk found" : "LeftDisk NOT found"));
        }

        if (rightDisk == null)
        {
            rightDisk = GameObject.Find("RightDisk");
            Debug.Log("GameManager: " + (rightDisk != null ? "RightDisk found" : "RightDisk NOT found"));
        }

        // Get disk rotation components
        if (leftDisk != null)
        {
            leftDiskRotation = leftDisk.GetComponent<DiskRotation>();
            if (leftDiskRotation == null)
            {
                leftDiskRotation = leftDisk.AddComponent<DiskRotation>();
                leftDiskRotation.isLeftDisk = true;
                leftDiskRotation.gameManager = this;
                Debug.Log("GameManager: Added DiskRotation to LeftDisk");
            }
            else
            {
                leftDiskRotation.isLeftDisk = true;
                leftDiskRotation.gameManager = this;
            }
        }

        if (rightDisk != null)
        {
            rightDiskRotation = rightDisk.GetComponent<DiskRotation>();
            if (rightDiskRotation == null)
            {
                rightDiskRotation = rightDisk.AddComponent<DiskRotation>();
                rightDiskRotation.isLeftDisk = false;
                rightDiskRotation.gameManager = this;
                Debug.Log("GameManager: Added DiskRotation to RightDisk");
            }
            else
            {
                rightDiskRotation.isLeftDisk = false;
                rightDiskRotation.gameManager = this;
            }
        }

        // Find UI elements through uiManager if available
        if (uiManager != null)
        {
            if (targetNumberText == null)
                targetNumberText = uiManager.targetNumberText;

            if (selectedNumberText == null)
                selectedNumberText = uiManager.selectedNumberText;

            if (operatorImage == null)
                operatorImage = uiManager.operatorImage;
        }
        else
        {
            // Try to find them directly
            if (targetNumberText == null)
                targetNumberText = GameObject.Find("target_number")?.GetComponent<TextMeshProUGUI>();

            if (selectedNumberText == null)
                selectedNumberText = GameObject.Find("selected_number")?.GetComponent<TextMeshProUGUI>();

            if (operatorImage == null)
                operatorImage = GameObject.Find("OperatorImage")?.GetComponent<Image>();
        }

        // Find sprites if needed
        if (additionSprite == null || subtractionSprite == null ||
            multiplicationSprite == null || divisionSprite == null)
        {
            if (uiManager != null)
            {
                additionSprite = uiManager.additionSprite;
                subtractionSprite = uiManager.subtractionSprite;
                multiplicationSprite = uiManager.multiplicationSprite;
                divisionSprite = uiManager.divisionSprite;
            }
        }

        // Find text components if not assigned
        FindTextComponents();
    }

    // Find all text components on the disks
    private void FindTextComponents()
    {
        bool needLeftTexts = false;
        bool needRightTexts = false;

        // Check if we need to find texts
        for (int i = 0; i < 5; i++)
        {
            if (leftDiskTexts[i] == null) needLeftTexts = true;
            if (rightDiskTexts[i] == null) needRightTexts = true;
        }

        // Find left disk texts if needed
        if (needLeftTexts && leftDisk != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform textTransform = leftDisk.transform.Find("leftDiskText" + (i + 1));
                if (textTransform != null)
                {
                    leftDiskTexts[i] = textTransform.GetComponent<TextMeshPro>();
                    Debug.Log($"GameManager: Found leftDiskText{i + 1}: {(leftDiskTexts[i] != null ? "success" : "FAILED - no TextMeshPro component")}");

                    // Assign to DiskRotation as well
                    if (leftDiskRotation != null && leftDiskTexts[i] != null)
                    {
                        if (leftDiskRotation.numberTexts == null) leftDiskRotation.numberTexts = new TextMeshPro[5];
                        leftDiskRotation.numberTexts[i] = leftDiskTexts[i];
                    }
                }
                else
                {
                    Debug.LogError($"GameManager: Could not find leftDiskText{i + 1} child transform!");
                }
            }
        }

        // Find right disk texts if needed
        if (needRightTexts && rightDisk != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform textTransform = rightDisk.transform.Find("rightDiskText" + (i + 1));
                if (textTransform != null)
                {
                    rightDiskTexts[i] = textTransform.GetComponent<TextMeshPro>();
                    Debug.Log($"GameManager: Found rightDiskText{i + 1}: {(rightDiskTexts[i] != null ? "success" : "FAILED - no TextMeshPro component")}");

                    // Assign to DiskRotation as well
                    if (rightDiskRotation != null && rightDiskTexts[i] != null)
                    {
                        if (rightDiskRotation.numberTexts == null) rightDiskRotation.numberTexts = new TextMeshPro[5];
                        rightDiskRotation.numberTexts[i] = rightDiskTexts[i];
                    }
                }
                else
                {
                    Debug.LogError($"GameManager: Could not find rightDiskText{i + 1} child transform!");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update timer
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (uiManager != null)
            {
                uiManager.UpdateTimer(timeLeft);
            }
        }
        else
        {
            // Game over
            GameOver();
        }
    }

    // Initialize game based on selected operation and difficulty
    public void InitializeGame(Operation operation, Difficulty difficulty)
    {
        Debug.Log($"GameManager: InitializeGame called with op={operation}, diff={difficulty}");
        currentOperation = operation;
        currentDifficulty = difficulty;
        currentScore = 0;

        // Set time based on difficulty
        switch (difficulty)
        {
            case Difficulty.Easy:
                timeLeft = 90f;
                break;
            case Difficulty.Medium:
                timeLeft = 60f;
                break;
            case Difficulty.Hard:
                timeLeft = 45f;
                break;
        }

        // Update UI displays
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
            uiManager.UpdateTimer(timeLeft);

            // Force update the operator image
            uiManager.SetOperatorImage(currentOperation);
        }
        else
        {
            // Update operator image directly if uiManager is not available
            SetOperatorImageDirect(currentOperation);
        }

        // Make sure we have all references
        FindReferences();

        // Reset disk rotations
        if (leftDiskRotation != null) leftDiskRotation.ResetDisk();
        if (rightDiskRotation != null) rightDiskRotation.ResetDisk();

        // Generate numbers for disks
        GenerateNumbersForDisks();

        // Generate a valid target number
        GenerateTargetNumber();

        // Force update of selected numbers to ensure display is correct
        UpdateSelectedNumbers();
    }

    // Set operator image directly if uiManager is not available
    private void SetOperatorImageDirect(Operation operation)
    {
        if (operatorImage != null)
        {
            switch (operation)
            {
                case Operation.Addition:
                    if (additionSprite != null)
                        operatorImage.sprite = additionSprite;
                    break;

                case Operation.Subtraction:
                    if (subtractionSprite != null)
                        operatorImage.sprite = subtractionSprite;
                    break;

                case Operation.Multiplication:
                    if (multiplicationSprite != null)
                        operatorImage.sprite = multiplicationSprite;
                    break;

                case Operation.Division:
                    if (divisionSprite != null)
                        operatorImage.sprite = divisionSprite;
                    break;
            }

            Debug.Log($"GameManager: Set operator image to {operation}");
        }
        else
        {
            Debug.LogError("GameManager: operatorImage is null in SetOperatorImageDirect!");
        }
    }

    // Generate valid numbers for both disks based on operation and difficulty
    public void GenerateNumbersForDisks()
    {
        Debug.Log("GameManager: GenerateNumbersForDisks called");

        // Check if we have all necessary references
        bool hasAllReferences = true;
        for (int i = 0; i < 5; i++)
        {
            if (leftDiskTexts[i] == null)
            {
                Debug.LogError($"GameManager: leftDiskTexts[{i}] is null!");
                hasAllReferences = false;
            }
            if (rightDiskTexts[i] == null)
            {
                Debug.LogError($"GameManager: rightDiskTexts[{i}] is null!");
                hasAllReferences = false;
            }
        }

        if (!hasAllReferences)
        {
            Debug.LogError("GameManager: Missing text references, trying to find them...");
            FindTextComponents();

            // Check again if we found all references
            for (int i = 0; i < 5; i++)
            {
                if (leftDiskTexts[i] == null || rightDiskTexts[i] == null)
                {
                    Debug.LogError("GameManager: Still missing text references after Find attempt!");
                    return;
                }
            }
        }

        int minValue = difficultyRanges[currentDifficulty].min;
        int maxValue = difficultyRanges[currentDifficulty].max;

        Debug.Log($"GameManager: Generating numbers with range {minValue}-{maxValue} for {currentOperation}");

        int[] leftNumbers = new int[5];
        int[] rightNumbers = new int[5];

        // First, create one valid pair that will work for the target number
        CreateValidNumberPair(ref leftNumbers[0], ref rightNumbers[0], minValue, maxValue);

        // Generate the rest of the numbers
        for (int i = 1; i < 5; i++)
        {
            // Generate appropriate numbers based on operation
            switch (currentOperation)
            {
                case Operation.Addition:
                    // For addition, we can use any numbers within the range
                    leftNumbers[i] = Random.Range(minValue, maxValue + 1);
                    rightNumbers[i] = Random.Range(minValue, maxValue + 1);
                    break;

                case Operation.Subtraction:
                    // For subtraction, ensure left numbers are greater than or equal to right numbers to avoid negative results
                    leftNumbers[i] = Random.Range(minValue + 5, maxValue + 1);
                    rightNumbers[i] = Random.Range(minValue, leftNumbers[i] + 1);
                    break;

                case Operation.Multiplication:
                    // For multiplication, use smaller numbers for harder difficulties
                    int multiMin = Mathf.Max(1, minValue / 2);
                    int multiMax = Mathf.Max(5, maxValue / 2);

                    leftNumbers[i] = Random.Range(multiMin, multiMax + 1);
                    rightNumbers[i] = Random.Range(multiMin, multiMax + 1);
                    break;

                case Operation.Division:
                    // For division, ensure all divisions result in whole numbers (no remainders)
                    rightNumbers[i] = Random.Range(1, Mathf.Min(10, maxValue) + 1);
                    int multiplier = Random.Range(1, Mathf.Max(3, maxValue / rightNumbers[i]) + 1);
                    leftNumbers[i] = rightNumbers[i] * multiplier;
                    break;
            }
        }

        // Shuffle the arrays so the valid pair isn't always at index 0
        ShuffleArray(leftNumbers);
        ShuffleArray(rightNumbers);

        // Set the numbers on the disks
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (leftDiskTexts[i] != null)
                {
                    leftDiskTexts[i].text = leftNumbers[i].ToString();
                    Debug.Log($"GameManager: Set leftDiskTexts[{i}] to '{leftNumbers[i]}'");
                }

                if (rightDiskTexts[i] != null)
                {
                    rightDiskTexts[i].text = rightNumbers[i].ToString();
                    Debug.Log($"GameManager: Set rightDiskTexts[{i}] to '{rightNumbers[i]}'");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameManager: Error setting disk text: {e.Message}");
            }
        }

        // Update selected numbers
        UpdateSelectedNumbers();
    }

    // Create a valid pair of numbers for the current operation
    private void CreateValidNumberPair(ref int leftNumber, ref int rightNumber, int minValue, int maxValue)
    {
        switch (currentOperation)
        {
            case Operation.Addition:
                // Target will be between minValue*2 and maxValue*2
                int targetSum = Random.Range(minValue * 2, maxValue * 2 + 1);
                leftNumber = Random.Range(minValue, maxValue + 1);
                rightNumber = targetSum - leftNumber;

                // Ensure rightNumber is in range
                if (rightNumber < minValue || rightNumber > maxValue)
                {
                    // Try again with a different target
                    rightNumber = Random.Range(minValue, maxValue + 1);
                    leftNumber = targetSum - rightNumber;

                    // If still out of range, just use random values
                    if (leftNumber < minValue || leftNumber > maxValue)
                    {
                        leftNumber = Random.Range(minValue, maxValue + 1);
                        rightNumber = Random.Range(minValue, maxValue + 1);
                    }
                }

                targetNumber = leftNumber + rightNumber;
                break;

            case Operation.Subtraction:
                // Ensure left > right to avoid negative results
                leftNumber = Random.Range(minValue + 5, maxValue + 1);
                rightNumber = Random.Range(minValue, leftNumber + 1);
                targetNumber = leftNumber - rightNumber;
                break;

            case Operation.Multiplication:
                // Use smaller numbers for multiplication
                int multiMin = Mathf.Max(1, minValue / 2);
                int multiMax = Mathf.Max(5, maxValue / 2);

                leftNumber = Random.Range(multiMin, multiMax + 1);
                rightNumber = Random.Range(multiMin, multiMax + 1);
                targetNumber = leftNumber * rightNumber;
                break;

            case Operation.Division:
                // Ensure clean division (no remainders)
                rightNumber = Random.Range(1, Mathf.Min(10, maxValue) + 1);
                int multiplier = Random.Range(1, Mathf.Max(3, maxValue / rightNumber) + 1);
                leftNumber = rightNumber * multiplier;
                targetNumber = multiplier; // leftNumber / rightNumber
                break;
        }

        Debug.Log($"GameManager: Created valid pair - Left: {leftNumber}, Right: {rightNumber}, Target: {targetNumber}");
    }

    // Fisher-Yates shuffle algorithm
    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    // Generate a valid target number based on current disk values
    private void GenerateTargetNumber()
    {
        Debug.Log("GameManager: GenerateTargetNumber called");

        // Target number is already set in CreateValidNumberPair

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateTargetNumber(targetNumber);
        }
        else if (targetNumberText != null)
        {
            targetNumberText.text = "Target: " + targetNumber;
            Debug.Log($"GameManager: Set targetNumberText to 'Target: {targetNumber}'");
        }
        else
        {
            Debug.LogError("GameManager: Unable to display target number - no UI references!");
        }
    }

    public void UpdateSelectedNumbers()
    {
        Debug.Log($"GameManager: UpdateSelectedNumbers called, indices: left={selectedLeftIndex}, right={selectedRightIndex}");

        try
        {
            string leftText = leftDiskTexts[selectedLeftIndex]?.text;
            string rightText = rightDiskTexts[selectedRightIndex]?.text;

            Debug.Log($"GameManager: Selected texts are '{leftText}' and '{rightText}'");

            if (string.IsNullOrEmpty(leftText) || string.IsNullOrEmpty(rightText))
            {
                Debug.LogError("GameManager: Selected text is empty! Using default values.");
                selectedLeftNumber = 5;
                selectedRightNumber = 3;
            }
            else
            {
                // Parse with error handling
                if (!int.TryParse(leftText, out selectedLeftNumber))
                {
                    Debug.LogError($"GameManager: Failed to parse '{leftText}' as integer! Using default value 5.");
                    selectedLeftNumber = 5;
                }

                if (!int.TryParse(rightText, out selectedRightNumber))
                {
                    Debug.LogError($"GameManager: Failed to parse '{rightText}' as integer! Using default value 3.");
                    selectedRightNumber = 3;
                }
            }

            int result = CalculateResult(selectedLeftNumber, selectedRightNumber, currentOperation);
            Debug.Log($"GameManager: Calculated result {result} from {selectedLeftNumber} and {selectedRightNumber}");

            // Update UI with both numbers and the result
            if (uiManager != null)
            {
                // Modified to show both selected numbers
                uiManager.UpdateSelectedNumberDisplay(selectedLeftNumber, selectedRightNumber, result);
            }
            else if (selectedNumberText != null)
            {
                // Display both selected numbers and the operation result
                string operatorSymbol = GetOperatorSymbol(currentOperation);
                selectedNumberText.text = $"Selected: {selectedLeftNumber} {operatorSymbol} {selectedRightNumber} = {result}";
                Debug.Log($"GameManager: Set selectedNumberText to '{selectedNumberText.text}'");
            }
            else
            {
                Debug.LogError("GameManager: Unable to display selected number - no UI references!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameManager: Error in UpdateSelectedNumbers: {e.Message}");
        }
    }

    private string GetOperatorSymbol(Operation op)
    {
        switch (op)
        {
            case Operation.Addition: return "+";
            case Operation.Subtraction: return "-";
            case Operation.Multiplication: return "×";
            case Operation.Division: return "÷";
            default: return "+";
        }
    }


    // Set the selected index for left disk (called from DiskRotation)
    public void SetSelectedLeftIndex(int index)
    {
        Debug.Log($"GameManager: SetSelectedLeftIndex called with index={index}");
        selectedLeftIndex = index;
        UpdateSelectedNumbers();
    }

    // Set the selected index for right disk (called from DiskRotation)
    public void SetSelectedRightIndex(int index)
    {
        Debug.Log($"GameManager: SetSelectedRightIndex called with index={index}");
        selectedRightIndex = index;
        UpdateSelectedNumbers();
    }

    // For direct number updates (compatibility with your original code)
    public void UpdateLeftNumber(int number)
    {
        Debug.Log($"GameManager: UpdateLeftNumber called with number={number}");
        selectedLeftNumber = number;
        UpdateResult();
    }

    public void UpdateRightNumber(int number)
    {
        Debug.Log($"GameManager: UpdateRightNumber called with number={number}");
        selectedRightNumber = number;
        UpdateResult();
    }

    private void UpdateResult()
    {
        int result = CalculateResult(selectedLeftNumber, selectedRightNumber, currentOperation);

        // Update UI with the result
        if (uiManager != null)
        {
            uiManager.UpdateSelectedNumber(result);
        }
        else if (selectedNumberText != null)
        {
            selectedNumberText.text = "Selected: " + result;
        }
    }

    // Calculate result based on operation
    private int CalculateResult(int left, int right, Operation op)
    {
        switch (op)
        {
            case Operation.Addition:
                return left + right;

            case Operation.Subtraction:
                return left - right;

            case Operation.Multiplication:
                return left * right;

            case Operation.Division:
                if (right == 0) return 0; // Avoid division by zero
                return left / right;

            default:
                return 0;
        }
    }

    // Check if the current selection matches the target
    public void CheckAnswer()
    {
        Debug.Log("GameManager: CheckAnswer called");

        int result = CalculateResult(selectedLeftNumber, selectedRightNumber, currentOperation);

        if (result == targetNumber)
        {
            // Correct answer
            Debug.Log("GameManager: Correct answer!");

            AudioSource correctSound = null;
            Transform audioTransform = transform.parent?.parent?.Find("Audio/correct");
            if (audioTransform == null)
            {
                GameObject audioObj = GameObject.Find("Audio/correct");
                if (audioObj != null)
                {
                    correctSound = audioObj.GetComponent<AudioSource>();
                }
            }
            else
            {
                correctSound = audioTransform.GetComponent<AudioSource>();
            }

            if (correctSound != null) correctSound.Play();

            int pointsToAdd = 0;

            // Add points based on difficulty
            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    pointsToAdd = 10;
                    break;
                case Difficulty.Medium:
                    pointsToAdd = 20;
                    break;
                case Difficulty.Hard:
                    pointsToAdd = 30;
                    break;
            }

            currentScore += pointsToAdd;

            if (uiManager != null)
            {
                uiManager.UpdateScore(currentScore);
            }

            // Generate new numbers and target
            GenerateNumbersForDisks();

            // No need to call GenerateTargetNumber() here since it's now created in GenerateNumbersForDisks()
        }
        else
        {
            // Wrong answer
            Debug.Log("GameManager: Wrong answer!");

            AudioSource wrongSound = null;
            Transform audioTransform = transform.parent?.parent?.Find("Audio/wrong");
            if (audioTransform == null)
            {
                GameObject audioObj = GameObject.Find("Audio/wrong");
                if (audioObj != null)
                {
                    wrongSound = audioObj.GetComponent<AudioSource>();
                }
            }
            else
            {
                wrongSound = audioTransform.GetComponent<AudioSource>();
            }

            if (wrongSound != null) wrongSound.Play();

            // Penalize by reducing time
            timeLeft = Mathf.Max(1f, timeLeft - 5f);

            if (uiManager != null)
            {
                uiManager.UpdateTimer(timeLeft);
            }
        }
    }

    // Game over function
    private void GameOver()
    {
        Debug.Log("GameManager: GameOver called");

        // Save high score if needed
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }

        // Show game over panel
        if (uiManager != null)
        {
            uiManager.ShowGameOverPanel(currentScore, highScore);
        }
        else
        {
            Debug.LogError("GameManager: UI_Manager reference is null in GameOver!");
        }
    }

    // Pause the game
    public void PauseGame()
    {
        Debug.Log("GameManager: PauseGame called");
        Time.timeScale = 0f;

        if (uiManager != null)
        {
            uiManager.ShowPauseScreen(true);
        }
    }

    // Resume the game
    public void ResumeGame()
    {
        Debug.Log("GameManager: ResumeGame called");
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.ShowPauseScreen(false);
        }
    }

    // Restart the game
    public void RestartGame()
    {
        Debug.Log("GameManager: RestartGame called");
        Time.timeScale = 1f;
        InitializeGame(currentOperation, currentDifficulty);
    }
}