using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BudgetUIManager : MonoBehaviour
{
    public ObjectDatabase objectDatabase; // Reference to the dataset
    public TMP_InputField budgetInput;   // User budget input field
    public TMP_Dropdown categoryDropdown; // Category selection dropdown
    public Transform resultsContent;     // Parent for displaying results
    public GameObject resultTemplate;    // Template for displaying an object
    public Button findButton;

    private void Start()
    {
        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabase is not assigned in the Inspector!");
            return;
        }

        PopulateDropdown();
        findButton.onClick.AddListener(OnFindBestOptions);
    }

    void PopulateDropdown()
    {
        if (categoryDropdown == null || objectDatabase == null)
        {
            Debug.LogError("CategoryDropdown or ObjectDatabase is not assigned!");
            return;
        }

        categoryDropdown.ClearOptions();
        List<string> categories = new List<string> { "All" };

        foreach (var obj in objectDatabase.objectsList)
        {
            if (!categories.Contains(obj.category))
                categories.Add(obj.category);
        }

        categoryDropdown.AddOptions(categories);
        categoryDropdown.value = 0;
        categoryDropdown.RefreshShownValue();

        if (categoryDropdown.template == null)
        {
            Debug.LogError("Dropdown template is not assigned!");
            return;
        }

        var dropdownContent = categoryDropdown.template.transform.Find("Viewport/Content");
        if (dropdownContent == null)
        {
            Debug.LogError("Dropdown content not found!");
            return;
        }
    }

    public void ResetUI()
    {
        // Reset dropdown to "All"
        if (categoryDropdown != null)
        {
            categoryDropdown.value = 0;
            categoryDropdown.RefreshShownValue();
        }

        // Clear budget input
        if (budgetInput != null)
        {
            budgetInput.text = "";
        }

        // Clear results
        ClearResults();
    }

    public void OnFindBestOptions()
    {
        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabase is not assigned!");
            return;
        }

        float budget;
        if (!float.TryParse(budgetInput.text, out budget))
        {
            Debug.LogError("Invalid budget entered!");
            return;
        }

        string selectedCategory = categoryDropdown.options[categoryDropdown.value].text;
        List<ObjectData> filteredObjects = objectDatabase.objectsList.FindAll(obj =>
            obj.price <= budget &&
            (selectedCategory == "All" || obj.category == selectedCategory));

        DisplayResults(filteredObjects);
    }

    void DisplayResults(List<ObjectData> filteredObjects)
    {
        if (resultsContent == null || resultTemplate == null) return;

        ClearResults();

        if (filteredObjects.Count == 0)
        {
            Debug.LogWarning("No objects found within budget!");
            return;
        }

        foreach (var obj in filteredObjects)
        {
            GameObject newResult = Instantiate(resultTemplate, resultsContent);
            newResult.transform.SetParent(resultsContent, false);
            newResult.SetActive(true);

            Transform resultImage = newResult.transform.Find("ResultImage");
            Transform resultText = newResult.transform.Find("ResultText");

            if (resultImage != null)
            {
                resultImage.gameObject.SetActive(true);
                Image imageComponent = resultImage.GetComponent<Image>();
                if (imageComponent != null && obj.image != null)
                {
                    imageComponent.sprite = obj.image;
                    imageComponent.enabled = true;
                }
            }

            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                TextMeshProUGUI textComponent = resultText.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"{obj.objectName}  \nRs {obj.price}";
                    textComponent.enabled = true;
                }
            }
        }
    }

    public void ClearResults()
    {
        if (resultsContent == null) return;

        foreach (Transform child in resultsContent)
        {
            Destroy(child.gameObject);
        }
    }
}