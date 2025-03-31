using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase;
using UnityEngine.UI;
using System.Collections;

public class ProfileManager : MonoBehaviour
{
    [Header("Profile Display")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text emailText;
    
    [Header("Edit Panel")]
    [SerializeField] private Button editNameButton;
    [SerializeField] private GameObject editNamePanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button saveNameButton;
    [SerializeField] private Button cancelNameButton;
    [SerializeField] private TMP_Text editStatusText;
    
    [Header("Loading Indicator")]
    [SerializeField] private GameObject loadingIndicator;
    
    private FirebaseUser user;

    private void Awake()
    {
        // Initialize button listeners
        editNameButton.onClick.AddListener(OpenEditPanel);
        saveNameButton.onClick.AddListener(SaveNameChanges);
        cancelNameButton.onClick.AddListener(CloseEditPanel);
    }

    private void Start()
    {
        InitializeProfile();
    }

    private void InitializeProfile()
    {
        loadingIndicator.SetActive(true);
        
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        
        if (user != null)
        {
            LoadProfileData();
        }
        else
        {
            Debug.LogError("No user logged in");
            // Optional: Redirect to login scene
        }
        
        loadingIndicator.SetActive(false);
    }

    private void LoadProfileData()
    {
        nameText.text = user.DisplayName ?? "No name set";
        emailText.text = user.Email ?? "No email available";
    }

    public void OpenEditPanel()
    {
        nameInputField.text = user.DisplayName;
        editStatusText.text = "";
        editNamePanel.SetActive(true);
    }

    public void SaveNameChanges()
    {
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            editStatusText.text = "Name cannot be empty";
            editStatusText.color = Color.red;
            return;
        }

        if (nameInputField.text == user.DisplayName)
        {
            editStatusText.text = "No changes detected";
            editStatusText.color = Color.yellow;
            return;
        }

        StartCoroutine(UpdateDisplayName(nameInputField.text));
    }

    private IEnumerator UpdateDisplayName(string newName)
    {
        editStatusText.text = "Saving...";
        editStatusText.color = Color.yellow;
        loadingIndicator.SetActive(true);
        saveNameButton.interactable = false;

        UserProfile profile = new UserProfile { DisplayName = newName };
        var updateTask = user.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.IsFaulted)
        {
            editStatusText.text = "Update failed: " + GetFirebaseError(updateTask.Exception);
            editStatusText.color = Color.red;
            Debug.LogError(updateTask.Exception);
        }
        else
        {
            editStatusText.text = "Updated successfully!";
            editStatusText.color = Color.green;
            
            // Update UI and references
            nameText.text = newName;
            References.userName = newName;
            
            // Close panel after delay
            yield return new WaitForSeconds(1f);
            editNamePanel.SetActive(false);
        }

        saveNameButton.interactable = true;
        loadingIndicator.SetActive(false);
    }

   private string GetFirebaseError(System.AggregateException exception)
{
    // Now properly recognizes FirebaseException
    FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
    
    if (firebaseEx != null)
    {
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
        
        // Convert error code to user-friendly message
        return errorCode switch
        {
            AuthError.InvalidEmail => "Invalid email address",
            AuthError.WeakPassword => "Password is too weak",
            AuthError.EmailAlreadyInUse => "Email already in use",
            AuthError.NetworkRequestFailed => "Network connection failed",
            _ => "Firebase error: " + errorCode
        };
    }
    return "Unknown error occurred";
}

    public void CloseEditPanel()
    {
        editNamePanel.SetActive(false);
    }

    // Call this when you need to refresh profile data
    public void RefreshProfile()
    {
        StartCoroutine(ReloadUserProfile());
    }

    private IEnumerator ReloadUserProfile()
    {
        loadingIndicator.SetActive(true);
        var reloadTask = user.ReloadAsync();

        yield return new WaitUntil(() => reloadTask.IsCompleted);

        if (reloadTask.IsFaulted)
        {
            Debug.LogError("Reload failed: " + reloadTask.Exception);
        }
        else
        {
            LoadProfileData();
        }

        loadingIndicator.SetActive(false);
    }
}