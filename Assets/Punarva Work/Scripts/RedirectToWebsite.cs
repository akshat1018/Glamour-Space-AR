using UnityEngine;
using UnityEngine.UI;

public class RedirectToWebsite : MonoBehaviour
{
    // The URL to redirect to
    private string websiteURL = "https://glamour-space.onrender.com/";

    // Reference to the button
    public Button redirectButton;

    void Start()
    {
        // Ensure the button is assigned
        if (redirectButton != null)
        {
            // Add a listener to the button's onClick event
            redirectButton.onClick.AddListener(OpenWebsite);
        }
        else
        {
            Debug.LogError("Redirect Button is not assigned in the Inspector.");
        }
    }

    // Method to open the website
    void OpenWebsite()
    {
        // Open the URL in the default web browser
        Application.OpenURL(websiteURL);
    }
}