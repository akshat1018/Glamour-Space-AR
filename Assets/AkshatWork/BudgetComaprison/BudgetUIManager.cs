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

    // Populate Dropdown with unique categories
    void PopulateDropdown()
    {
        if (categoryDropdown == null || objectDatabase == null)
        {
            Debug.LogError("CategoryDropdown or ObjectDatabase is not assigned!");
            return;
        }

        // Clear existing options
        categoryDropdown.ClearOptions();

        // Create a list of unique categories, starting with "All"
        List<string> categories = new List<string> { "All" };

        // Populate the list with unique categories from the object database
        foreach (var obj in objectDatabase.objectsList)
        {
            if (!categories.Contains(obj.category))
                categories.Add(obj.category);
        }

        // Add the categories to the dropdown's options
        categoryDropdown.AddOptions(categories);

        // Ensure the dropdown's template is properly assigned
        if (categoryDropdown.template == null)
        {
            Debug.LogError("Dropdown template is not assigned!");
            return;
        }

        // Access the dropdown's content GameObject
        var dropdownContent = categoryDropdown.template.transform.Find("Viewport/Content");
        if (dropdownContent == null)
        {
            Debug.LogError("Dropdown content not found!");
            return;
        }

        Debug.Log("Dropdown populated with categories.");
    }

    // Function triggered on "Find Best Option" button click
    public void OnFindBestOptions()
    {
        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabase is not assigned!");
            return;
        }

        Debug.Log("Find Best Option Button Clicked");

        float budget;
        if (!float.TryParse(budgetInput.text, out budget))
        {
            Debug.LogError("Invalid budget entered!");
            return;
        }

        Debug.Log("Entered Budget: " + budget);

        string selectedCategory = categoryDropdown.options[categoryDropdown.value].text;
        Debug.Log("Selected Category: " + selectedCategory);

        List<ObjectData> filteredObjects = objectDatabase.objectsList.FindAll(obj =>
            obj.price <= budget &&
            (selectedCategory == "All" || obj.category == selectedCategory));

        Debug.Log("Filtered Objects Count: " + filteredObjects.Count);

        DisplayResults(filteredObjects);
    }

    // Display the filtered objects
    void DisplayResults(List<ObjectData> filteredObjects)
    {
        if (resultsContent == null)
        {
            Debug.LogError("ResultsContent is not assigned in the Inspector!");
            return;
        }

        if (resultTemplate == null)
        {
            Debug.LogError("ResultTemplate is not assigned in the Inspector!");
            return;
        }

        // Clear previous results
        ClearResults();

        Debug.Log("Displaying " + filteredObjects.Count + " objects in UI...");

        if (filteredObjects.Count == 0)
        {
            Debug.LogWarning("No objects found within budget!");
            return;
        }

        foreach (var obj in filteredObjects)
        {
            // Instantiate Result Template
            GameObject newResult = Instantiate(resultTemplate, resultsContent);
            newResult.transform.SetParent(resultsContent, false); // Fix UI scaling
            newResult.SetActive(true); // Ensure template is visible

            // Safeguard: Ensure the instance is not null
            if (newResult == null)
            {
                Debug.LogError("Failed to instantiate result template!");
                continue;
            }

            // 🔥 Fix: Ensure Image and Text are also enabled
            Transform resultImage = newResult.transform.Find("ResultImage");
            Transform resultText = newResult.transform.Find("ResultText");

            if (resultImage != null)
            {
                resultImage.gameObject.SetActive(true);
                Image imageComponent = resultImage.GetComponent<Image>();
                if (imageComponent != null && obj.image != null)
                {
                    imageComponent.sprite = obj.image;
                    imageComponent.enabled = true; // ✅ Ensure the Image component is enabled
                }
                else
                {
                    Debug.LogError("Missing Image for: " + obj.objectName);
                }
            }
            else
            {
                Debug.LogError("ResultImage not found in the template!");
            }

            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                TextMeshProUGUI textComponent = resultText.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"{obj.objectName}  \nRs {obj.price}";
                    textComponent.enabled = true; // ✅ Ensure the Text component is enabled
                }
                else
                {
                    Debug.LogError("Missing Text Component for: " + obj.objectName);
                }
            }
            else
            {
                Debug.LogError("ResultText not found in the template!");
            }
        }
    }

    // Clear all results from the resultsContent
    public void ClearResults()
    {
        if (resultsContent == null)
        {
            Debug.LogError("ResultsContent is not assigned!");
            return;
        }

        // Destroy all child objects in resultsContent
        foreach (Transform child in resultsContent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("✅ Results cleared.");
    }
}