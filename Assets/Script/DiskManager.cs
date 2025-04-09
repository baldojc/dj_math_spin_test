using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiskManager : MonoBehaviour
{
    // References to disk objects
    public GameObject leftDisk;
    public GameObject rightDisk;

    // References to disk rotation scripts
    private DiskRotation leftDiskRotation;
    private DiskRotation rightDiskRotation;

    // References to text elements
    public TextMeshPro[] leftDiskTexts = new TextMeshPro[5];
    public TextMeshPro[] rightDiskTexts = new TextMeshPro[5];

    // Reference to GameManager
    private GameManager gameManager;

    // Audio settings
    private bool isMuted = false;
    public AudioSource backgroundMusic;
    public AudioSource[] soundEffects;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("DiskManager: Start method called");

        // Find GameManager
        gameManager = GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("DiskManager: GameManager not found!");
            }
        }

        // Find disks if not assigned
        if (leftDisk == null)
        {
            leftDisk = transform.parent.Find("LeftDisk")?.gameObject;
            if (leftDisk == null)
            {
                leftDisk = GameObject.Find("LeftDisk");
                if (leftDisk == null)
                {
                    Debug.LogError("DiskManager: LeftDisk not found!");
                }
            }
        }

        if (rightDisk == null)
        {
            rightDisk = transform.parent.Find("RightDisk")?.gameObject;
            if (rightDisk == null)
            {
                rightDisk = GameObject.Find("RightDisk");
                if (rightDisk == null)
                {
                    Debug.LogError("DiskManager: RightDisk not found!");
                }
            }
        }

        // Get disk rotation scripts
        if (leftDisk != null)
        {
            leftDiskRotation = leftDisk.GetComponent<DiskRotation>();
            if (leftDiskRotation == null)
            {
                leftDiskRotation = leftDisk.AddComponent<DiskRotation>();
                leftDiskRotation.isLeftDisk = true;
                leftDiskRotation.gameManager = gameManager;
                Debug.Log("DiskManager: Added DiskRotation to LeftDisk");
            }
            else
            {
                // Ensure properties are set correctly
                leftDiskRotation.isLeftDisk = true;
                if (leftDiskRotation.gameManager == null)
                    leftDiskRotation.gameManager = gameManager;
            }
        }

        if (rightDisk != null)
        {
            rightDiskRotation = rightDisk.GetComponent<DiskRotation>();
            if (rightDiskRotation == null)
            {
                rightDiskRotation = rightDisk.AddComponent<DiskRotation>();
                rightDiskRotation.isLeftDisk = false;
                rightDiskRotation.gameManager = gameManager;
                Debug.Log("DiskManager: Added DiskRotation to RightDisk");
            }
            else
            {
                // Ensure properties are set correctly
                rightDiskRotation.isLeftDisk = false;
                if (rightDiskRotation.gameManager == null)
                    rightDiskRotation.gameManager = gameManager;
            }
        }

        // Find Text elements if not assigned
        FindDiskTextElements();

        // Set up background music if available
        if (backgroundMusic == null)
        {
            GameObject audioObj = GameObject.Find("Main_Menu_Panel/Audio Source");
            if (audioObj != null)
            {
                backgroundMusic = audioObj.GetComponent<AudioSource>();
                Debug.Log("DiskManager: Found background music");
            }
        }

        // Find sound effects if not assigned
        if (soundEffects == null || soundEffects.Length == 0)
        {
            GameObject audioParent = GameObject.Find("GameObject_2DObject/Audio");
            if (audioParent != null)
            {
                soundEffects = audioParent.GetComponentsInChildren<AudioSource>();
                Debug.Log($"DiskManager: Found {soundEffects.Length} sound effects");
            }
        }

        // Load saved audio settings
        LoadAudioSettings();
    }

    // Find all text elements on disks
    private void FindDiskTextElements()
    {
        Debug.Log("DiskManager: Finding disk text elements");

        if (leftDisk != null)
        {
            bool foundAllLeft = true;
            for (int i = 0; i < 5; i++)
            {
                if (leftDiskTexts[i] == null)
                {
                    Transform textTransform = leftDisk.transform.Find($"leftDiskText{i + 1}");
                    if (textTransform != null)
                    {
                        leftDiskTexts[i] = textTransform.GetComponent<TextMeshPro>();
                        if (leftDiskTexts[i] == null)
                        {
                            Debug.LogError($"DiskManager: leftDiskText{i + 1} has no TextMeshPro component!");
                            foundAllLeft = false;
                        }
                    }
                    else
                    {
                        Debug.LogError($"DiskManager: leftDiskText{i + 1} not found!");
                        foundAllLeft = false;
                    }
                }
            }

            Debug.Log($"DiskManager: {(foundAllLeft ? "Successfully found" : "Failed to find")} all left disk texts");
        }

        if (rightDisk != null)
        {
            bool foundAllRight = true;
            for (int i = 0; i < 5; i++)
            {
                if (rightDiskTexts[i] == null)
                {
                    Transform textTransform = rightDisk.transform.Find($"rightDiskText{i + 1}");
                    if (textTransform != null)
                    {
                        rightDiskTexts[i] = textTransform.GetComponent<TextMeshPro>();
                        if (rightDiskTexts[i] == null)
                        {
                            Debug.LogError($"DiskManager: rightDiskText{i + 1} has no TextMeshPro component!");
                            foundAllRight = false;
                        }
                    }
                    else
                    {
                        Debug.LogError($"DiskManager: rightDiskText{i + 1} not found!");
                        foundAllRight = false;
                    }
                }
            }

            Debug.Log($"DiskManager: {(foundAllRight ? "Successfully found" : "Failed to find")} all right disk texts");
        }

        // Pass references to GameManager
        if (gameManager != null)
        {
            gameManager.leftDiskTexts = leftDiskTexts;
            gameManager.rightDiskTexts = rightDiskTexts;
        }
        else
        {
            Debug.LogError("DiskManager: Cannot pass text references - GameManager is null!");
        }

        // Also pass to disk rotation scripts
        if (leftDiskRotation != null)
        {
            leftDiskRotation.numberTexts = leftDiskTexts;
        }

        if (rightDiskRotation != null)
        {
            rightDiskRotation.numberTexts = rightDiskTexts;
        }
    }

    // Set the numbers on disk texts
    public void SetDiskNumbers(int[] leftNumbers, int[] rightNumbers)
    {
        Debug.Log("DiskManager: SetDiskNumbers called");

        for (int i = 0; i < 5; i++)
        {
            if (i < leftNumbers.Length && leftDiskTexts[i] != null)
            {
                leftDiskTexts[i].text = leftNumbers[i].ToString();
                Debug.Log($"DiskManager: Set leftDiskText{i + 1} to '{leftNumbers[i]}'");
            }

            if (i < rightNumbers.Length && rightDiskTexts[i] != null)
            {
                rightDiskTexts[i].text = rightNumbers[i].ToString();
                Debug.Log($"DiskManager: Set rightDiskText{i + 1} to '{rightNumbers[i]}'");
            }
        }
    }

    // Rotate left disk by UI button press
    public void RotateLeftDiskClockwise()
    {
        Debug.Log("DiskManager: RotateLeftDiskClockwise called");
        if (leftDiskRotation != null)
        {
            leftDiskRotation.RotateClockwise();
        }
        else
        {
            Debug.LogError("DiskManager: leftDiskRotation is null!");
        }
    }

    public void RotateLeftDiskCounterClockwise()
    {
        Debug.Log("DiskManager: RotateLeftDiskCounterClockwise called");
        if (leftDiskRotation != null)
        {
            leftDiskRotation.RotateCounterClockwise();
        }
        else
        {
            Debug.LogError("DiskManager: leftDiskRotation is null!");
        }
    }

    // Rotate right disk by UI button press
    public void RotateRightDiskClockwise()
    {
        Debug.Log("DiskManager: RotateRightDiskClockwise called");
        if (rightDiskRotation != null)
        {
            rightDiskRotation.RotateClockwise();
        }
        else
        {
            Debug.LogError("DiskManager: rightDiskRotation is null!");
        }
    }

    public void RotateRightDiskCounterClockwise()
    {
        Debug.Log("DiskManager: RotateRightDiskCounterClockwise called");
        if (rightDiskRotation != null)
        {
            rightDiskRotation.RotateCounterClockwise();
        }
        else
        {
            Debug.LogError("DiskManager: rightDiskRotation is null!");
        }
    }

    // Audio control methods
    public void ToggleMute()
    {
        Debug.Log("DiskManager: ToggleMute called");
        isMuted = !isMuted;

        // Apply mute state to background music
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isMuted;
        }

        // Apply mute state to sound effects
        if (soundEffects != null)
        {
            foreach (AudioSource source in soundEffects)
            {
                if (source != null)
                {
                    source.mute = isMuted;
                }
            }
        }

        // Save setting
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"DiskManager: Audio is now {(isMuted ? "muted" : "unmuted")}");
    }

    // Explicitly mute or unmute
    public void MuteAudio()
    {
        if (!isMuted)
        {
            ToggleMute();
        }
    }

    public void UnmuteAudio()
    {
        if (isMuted)
        {
            ToggleMute();
        }
    }

    // Load saved audio settings
    private void LoadAudioSettings()
    {
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        Debug.Log($"DiskManager: Loaded audio settings - isMuted={isMuted}");

        // Apply saved setting
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isMuted;
        }

        if (soundEffects != null)
        {
            foreach (AudioSource source in soundEffects)
            {
                if (source != null)
                {
                    source.mute = isMuted;
                }
            }
        }
    }

    // Show How To Play panel
    public void ShowHowToPlay()
    {
        Debug.Log("DiskManager: ShowHowToPlay called");
        // This would connect to a UI_Manager method to show the How To Play panel
        UI_Manager uiManager = FindObjectOfType<UI_Manager>();
        if (uiManager != null)
        {
            // Check if the method exists through reflection
            var methodInfo = uiManager.GetType().GetMethod("ShowHowToPlayPanel");
            if (methodInfo != null)
            {
                Debug.Log("DiskManager: Calling UI_Manager.ShowHowToPlayPanel");
                methodInfo.Invoke(uiManager, null);
            }
            else
            {
                Debug.LogWarning("DiskManager: UI_Manager does not have ShowHowToPlayPanel method");
            }
        }
        else
        {
            Debug.LogError("DiskManager: UI_Manager not found!");
        }
    }
}