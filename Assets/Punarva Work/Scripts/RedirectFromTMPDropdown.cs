using UnityEngine;
using TMPro;

public class RedirectFromTMPDropdown : MonoBehaviour
{
    // The URL to redirect to
    private string websiteURL = "https://glamour-space.onrender.com/";

    // Reference to the TextMeshPro Dropdown
    public TMP_Dropdown redirectDropdown;

    void Start()
    {
        // Ensure the dropdown is assigned
        if (redirectDropdown != null)
        {
            // Add a listener to the dropdown's onValueChanged event
            redirectDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        else
        {
            Debug.LogError("Redirect Dropdown (TextMeshPro) is not assigned in the Inspector.");
        }
    }

    // Method to handle dropdown value changes
    void OnDropdownValueChanged(int index)
    {
        // Check if the selected option is the one that should trigger the redirect
        if (index == 0 /*|| index == 1 || index == 2*/ ) // Assuming the second option (index 1) is the one that redirects
        {
            OpenWebsite();
        }
        /*if (index == 1) // Assuming the second option (index 1) is the one that redirects
        {
            OpenWebsite();
        }
        if (index == 2) // Assuming the second option (index 1) is the one that redirects
        {
            OpenWebsite();
        }*/
    }

    // Method to open the website
    void OpenWebsite()
    {
        // Open the URL in the default web browser
        Application.OpenURL(websiteURL);
    }
}