using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiskManager : MonoBehaviour
{
    [Header("Disk References")]
    public GameObject leftDisk;
    public GameObject rightDisk;
    public TextMeshProUGUI leftNumberText;
    public TextMeshProUGUI rightNumberText;

    [Header("Disk Texts")]
    public TextMeshProUGUI[] leftDiskTexts;
    public TextMeshProUGUI[] rightDiskTexts;

    [Header("Arrow Controls")]
    public Button leftArrowButton;
    public Button rightArrowButton;

    [Header("Rotation Settings")]
    public float rotationSpeed = 120f;
    public float snapDuration = 0.3f;

    [Header("Audio")]
    public AudioClip diskRotateSound;
    private AudioSource audioSource;

    // Disk data
    private List<int> leftDiskNumbers = new List<int>();
    private List<int> rightDiskNumbers = new List<int>();

    // Current position indices
    private int currentLeftIndex = 0;
    private int currentRightIndex = 0;

    // Rotation state
    private bool isLeftDiskRotating = false;
    private bool isRightDiskRotating = false;

    // Target rotation angles
    private float leftDiskTargetAngle = 0f;
    private float rightDiskTargetAngle = 0f;

    // Angle per number (for 5 numbers, each number takes 72 degrees)
    private const float ANGLE_PER_NUMBER = 72f;

    private void Start()
    {
        // Set up button listeners
        leftArrowButton.onClick.AddListener(RotateLeftDisk);
        rightArrowButton.onClick.AddListener(RotateRightDisk);

        // Get audio source component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Handle disk rotations
        HandleDiskRotation(leftDisk, ref isLeftDiskRotating, leftDiskTargetAngle);
        HandleDiskRotation(rightDisk, ref isRightDiskRotating, rightDiskTargetAngle);
    }

    // Set up numbers for both disks
    public void SetDiskNumbers(List<int> leftNumbers, List<int> rightNumbers)
    {
        // Make sure we have exactly 5 numbers for each disk
        if (leftNumbers.Count != 5 || rightNumbers.Count != 5)
        {
            Debug.LogError("Each disk must have exactly 5 numbers");
            return;
        }

        // Store the numbers
        leftDiskNumbers = new List<int>(leftNumbers);
        rightDiskNumbers = new List<int>(rightNumbers);

        // Reset disk positions
        currentLeftIndex = 0;
        currentRightIndex = 0;
        leftDisk.transform.rotation = Quaternion.identity;
        rightDisk.transform.rotation = Quaternion.identity;
        leftDiskTargetAngle = 0f;
        rightDiskTargetAngle = 0f;

        // Update text displays
        UpdateDiskTexts();
    }

    // Update the text elements on both disks
    private void UpdateDiskTexts()
    {
        // Update left disk texts
        for (int i = 0; i < leftDiskTexts.Length; i++)
        {
            leftDiskTexts[i].text = leftDiskNumbers[i].ToString();
        }

        // Update right disk texts
        for (int i = 0; i < rightDiskTexts.Length; i++)
        {
            rightDiskTexts[i].text = rightDiskNumbers[i].ToString();
        }

        // Update the current selection texts
        UpdateSelectionTexts();
    }

    // Update the selection display
    private void UpdateSelectionTexts()
    {
        leftNumberText.text = leftDiskNumbers[currentLeftIndex].ToString();
        rightNumberText.text = rightDiskNumbers[currentRightIndex].ToString();
    }

    // Rotate the left disk to the next number
    public void RotateLeftDisk()
    {
        if (isLeftDiskRotating) return;

        // Update the index
        currentLeftIndex = (currentLeftIndex + 1) % 5;

        // Set the target angle
        leftDiskTargetAngle -= ANGLE_PER_NUMBER;

        // Start rotation
        isLeftDiskRotating = true;

        // Play rotation sound
        PlayRotationSound();
    }

    // Rotate the right disk to the next number
    public void RotateRightDisk()
    {
        if (isRightDiskRotating) return;

        // Update the index
        currentRightIndex = (currentRightIndex + 1) % 5;

        // Set the target angle
        rightDiskTargetAngle -= ANGLE_PER_NUMBER;

        // Start rotation
        isRightDiskRotating = true;

        // Play rotation sound
        PlayRotationSound();
    }

    // Handle smooth disk rotation
    private void HandleDiskRotation(GameObject disk, ref bool isRotating, float targetAngle)
    {
        if (!isRotating) return;

        // Calculate current rotation in Euler angles
        Vector3 currentRotation = disk.transform.rotation.eulerAngles;

        // Calculate the target rotation
        Vector3 targetRotation = new Vector3(currentRotation.x, currentRotation.y, targetAngle);

        // Rotate the disk smoothly
        disk.transform.rotation = Quaternion.Slerp(
            disk.transform.rotation,
            Quaternion.Euler(targetRotation),
            rotationSpeed * Time.deltaTime
        );

        // Check if rotation is almost complete
        if (Mathf.Abs(Mathf.DeltaAngle(currentRotation.z, targetAngle)) < 0.1f)
        {
            // Snap to exact angle
            disk.transform.rotation = Quaternion.Euler(targetRotation);
            isRotating = false;

            // Update the selection texts
            UpdateSelectionTexts();
        }
    }

    // Play rotation sound
    private void PlayRotationSound()
    {
        if (audioSource != null && diskRotateSound != null)
        {
            audioSource.PlayOneShot(diskRotateSound);
        }
    }

    // Set the disk position directly (used by the DiskRotationController)
    public void SetLeftDiskPosition(int index)
    {
        if (index >= 0 && index < 5)
        {
            currentLeftIndex = index;
            UpdateSelectionTexts();
        }
    }

    // Set the right disk position directly (used by the DiskRotationController)
    public void SetRightDiskPosition(int index)
    {
        if (index >= 0 && index < 5)
        {
            currentRightIndex = index;
            UpdateSelectionTexts();
        }
    }

    // Get the currently selected number from the left disk
    public int GetSelectedLeftNumber()
    {
        return leftDiskNumbers[currentLeftIndex];
    }

    // Get the currently selected number from the right disk
    public int GetSelectedRightNumber()
    {
        return rightDiskNumbers[currentRightIndex];
    }
}