using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DiskNumberGuide : MonoBehaviour
{
    [SerializeField] private bool isLeftDisk = true;
    [SerializeField] private GameObject numberGuidePrefab;
    [SerializeField] private float radius = 120f;
    [SerializeField] private Transform guideParent;

    // Reference to existing guide objects (exactly 6)
    [SerializeField] private TextMeshProUGUI[] numberGuideTexts = new TextMeshProUGUI[6];

    // Current numbers from GameManager
    private int[] currentNumbers;

    private void Start()
    {
        // If no parent transform is specified, use this transform
        if (guideParent == null)
            guideParent = transform;

        // Find existing guide objects if not assigned
        if (AreGuidesEmpty())
        {
            FindExistingGuides();
        }

        RefreshGuides();
    }

    private bool AreGuidesEmpty()
    {
        for (int i = 0; i < numberGuideTexts.Length; i++)
        {
            if (numberGuideTexts[i] != null)
                return false;
        }
        return true;
    }

    private void FindExistingGuides()
    {
        // Look for children with "NumberGuide" in their name
        TextMeshProUGUI[] foundGuides = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < foundGuides.Length && i < 6; i++)
        {
            if (foundGuides[i].gameObject.name.Contains("NumberGuide"))
            {
                numberGuideTexts[i] = foundGuides[i];
            }
        }
    }

    public void RefreshGuides()
    {
        // Get current numbers from GameManager
        if (GameManager.Instance != null)
        {
            currentNumbers = GameManager.Instance.GetDiskNumbers(isLeftDisk);

            // Update text values in existing guides
            UpdateGuideTexts();
        }
    }

    private void UpdateGuideTexts()
    {
        if (currentNumbers == null || currentNumbers.Length == 0)
            return;

        // Update existing guide texts
        for (int i = 0; i < Mathf.Min(numberGuideTexts.Length, currentNumbers.Length); i++)
        {
            if (numberGuideTexts[i] != null)
            {
                numberGuideTexts[i].text = currentNumbers[i].ToString();
            }
        }
    }

    // Use this method if you need to completely recreate the guides
    // (Only needed if guides don't exist in the scene already)
    private void CreateNumberGuides()
    {
        // Only needed if you're creating guides at runtime
        // Based on your hierarchy image, it seems guides already exist in the scene

        if (currentNumbers != null && currentNumbers.Length > 0)
        {
            int count = Mathf.Min(6, currentNumbers.Length); // Limit to 6 guides
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                // Calculate position
                float angle = i * angleStep;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Sin(radians) * radius,
                    Mathf.Cos(radians) * radius,
                    0
                );

                // Create guide if it doesn't exist
                if (numberGuideTexts[i] == null)
                {
                    GameObject guideObj = Instantiate(numberGuidePrefab, guideParent);
                    guideObj.name = $"NumberGuide ({i + 1})"; // Match naming in hierarchy
                    guideObj.transform.localPosition = position;

                    // Set number text
                    TextMeshProUGUI textComponent = guideObj.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = currentNumbers[i].ToString();
                        numberGuideTexts[i] = textComponent;
                    }

                    // Adjust rotation to face outward
                    guideObj.transform.localRotation = Quaternion.Euler(0, 0, -angle);
                }
                // Update existing guide
                else
                {
                    numberGuideTexts[i].text = currentNumbers[i].ToString();
                }
            }
        }
    }

    // Call this when operation or difficulty changes
    public void OnOperationOrDifficultyChanged()
    {
        RefreshGuides();
    }
}