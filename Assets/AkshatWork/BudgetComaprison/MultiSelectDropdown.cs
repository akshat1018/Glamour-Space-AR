using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MultiSelectDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private List<string> selectedOptions = new List<string>();

    private void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedCategory = dropdown.options[index].text;

        if (selectedOptions.Contains(selectedCategory))
        {
            selectedOptions.Remove(selectedCategory);
        }
        else
        {
            selectedOptions.Add(selectedCategory);
        }

        UpdateDropdownLabel();
    }

    void UpdateDropdownLabel()
    {
        if (selectedOptions.Count == 0)
        {
            dropdown.captionText.text = "Select Categories";
        }
        else
        {
            dropdown.captionText.text = string.Join(", ", selectedOptions);
        }
    }

    public List<string> GetSelectedCategories()
    {
        return new List<string>(selectedOptions);
    }
}
