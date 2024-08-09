using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DropdownSceneChanger : MonoBehaviour
{
    public TMP_Dropdown tmpDropdown;

    void Start()
    {
        // Add listener for dropdown value change
        tmpDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(tmpDropdown); });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        // Check if the selected item is the 5th item (index 4, as index starts from 0)
        if (change.value == 4)
        {
            // Change to the desired scene
            SceneManager.LoadScene("3_Main Menu");  // Replace "YourSceneName" with your actual scene name
        }
    }
}
