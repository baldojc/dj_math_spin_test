using UnityEngine;
using UnityEngine.UI;

public class DifficultyButtonHandler : MonoBehaviour
{
    public GameManager.Difficulty difficulty;

    // Reference to the UIManager for starting the game
    public UIManager uiManager;

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Set the difficulty in the GameManager
        GameManager.Instance.SetDifficulty(difficulty);

        // Optionally, start the game if needed
        uiManager.StartGame();
    }
}
