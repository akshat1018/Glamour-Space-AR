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

    private void Start()
    {
        PopulateDropdown();
    }

    // Populate Dropdown with unique categories
    void PopulateDropdown()
    {
        categoryDropdown.ClearOptions();
        List<string> categories = new List<string> { "All" };

        foreach (var obj in objectDatabase.objectsList)
        {
            if (!categories.Contains(obj.category))
                categories.Add(obj.category);
        }

        categoryDropdown.AddOptions(categories);
    }

    // Function triggered on "Find Best Option" button click
    public void OnFindBestOptions()
{
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
    // Clear previous results
    foreach (Transform child in resultsContent)
    {
        Destroy(child.gameObject);
    }

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

        // 🔥 Fix: Ensure Image and Text are also enabled
        newResult.transform.Find("ResultImage").gameObject.SetActive(true);
        newResult.transform.Find("ResultText").gameObject.SetActive(true);

        Debug.Log("Created UI for: " + obj.objectName);

        // Set Image
        Image imageComponent = newResult.transform.Find("ResultImage").GetComponent<Image>();
        if (imageComponent != null && obj.image != null)
        {
            imageComponent.sprite = obj.image;
            imageComponent.enabled = true; // ✅ Ensure the Image component is enabled
        }
        else
        {
            Debug.LogError("Missing Image for: " + obj.objectName);
        }

        // Set Text
        TextMeshProUGUI textComponent = newResult.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"{obj.objectName} - ₹{obj.price}";
            textComponent.enabled = true; // ✅ Ensure the Text component is enabled
        }
        else
        {
            Debug.LogError("Missing Text Component for: " + obj.objectName);
        }
    }
}



}
