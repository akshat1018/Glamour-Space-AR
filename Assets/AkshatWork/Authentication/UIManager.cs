using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registrationPanel;

    [Space]
    [SerializeField]
    private GameObject emailVerificationPanel;

    [SerializeField]
    private TextMeshProUGUI emailVerificationText; // Changed to TextMeshProUGUI

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void ClearUI()
    {
        loginPanel.SetActive(false);
        registrationPanel.SetActive(false);
        emailVerificationPanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        ClearUI();
        loginPanel.SetActive(true);
    }

    public void OpenRegistrationPanel()
    {
        ClearUI();
        registrationPanel.SetActive(true);
    }

    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        ClearUI();
        emailVerificationPanel.SetActive(true);

        if(isEmailSent)
        {
            emailVerificationText.text = $"Please verify your email address \n Verification email has been sent to {emailId}";
        }
        else
        {
            emailVerificationText.text = $"Couldn't send email: {errorMessage}";
        }
    }
    public void ShowErrorMessage(string message)
{
    ClearUI();
    loginPanel.SetActive(true);
    // You might want to add an error text field in your login panel
    // errorText.text = message;
}
}
