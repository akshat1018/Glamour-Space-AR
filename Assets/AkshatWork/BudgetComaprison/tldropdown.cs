using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class tlDropdown : MonoBehaviour
{
    public TMP_Dropdown toolsDropdown; // Reference to Dropdown

    void Start()
    {
        // Ensure the dropdown selection event is assigned
        toolsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        // Check if the selected index is 1 (Budget Scene)
        if (index == 1)
        {
            Debug.Log("Switching to BudgetScene...");
            SceneManager.LoadScene("BudgetScene");
        }
    }
}
