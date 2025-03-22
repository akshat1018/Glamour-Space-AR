using System.Collections;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variable
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    // Login Variables
    [Space]
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    // Registration Variables
    [Space]
    [Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;

    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(()=> dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                yield return new WaitForEndOfFrame();
                StartCoroutine(CheckForAutoLogin());
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
            }
    }

    void InitializeFirebase()
    {
        // Set the default instance object
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckForAutoLogin()
    {
        if(user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(()=> reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            UIManager.Instance.OpenLoginPanel();
        }
    }

    private void AutoLogin()
    {
        if(user!= null)
        {
            References.userName = user.DisplayName;
            UnityEngine.SceneManagement.SceneManager.LoadScene("3_Main Menu");
        }
        
        else
        {
            UIManager.Instance.OpenLoginPanel();
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage = "Login Failed";
                    break;
            }

            Debug.Log(failedMessage);
        }
        else
        {
            // Change here to get the FirebaseUser from the AuthResult
            FirebaseUser user = loginTask.Result.User;

            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);

            if(user.IsEmailVerified)
            {
            References.userName = user.DisplayName;
            UnityEngine.SceneManagement.SceneManager.LoadScene("3_Main Menu");
            }
            else
            {
                SendEmailForVerification();
            }
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("User Name is empty");
        }
        else if (string.IsNullOrWhiteSpace(email))
        {
            Debug.LogError("Email field is empty");
        }
        else if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match");
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed! Because ";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is missing";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is missing";
                        break;
                    default:
                        failedMessage = "Registration Failed";
                        break;
                }

                Debug.Log(failedMessage);
            }
            else
            {
                // Get the User after registration success
                FirebaseUser user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile { DisplayName = name };

                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    // Delete the user if user update failed
                    user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failedMessage = "Profile update Failed! Because ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email is missing";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is missing";
                            break;
                        default:
                            failedMessage = "Profile update Failed";
                            break;
                    }

                    Debug.Log(failedMessage);
                }
                else
                {
                    Debug.Log("Registration Successful! Welcome " + user.DisplayName);
                    if(user.IsEmailVerified)
                    {
                        UIManager.Instance.OpenLoginPanel();
                    }
                    else
                    {
                        SendEmailForVerification();
                    }
                    UIManager.Instance.OpenLoginPanel();
                }
            }
        }
    }
    public void SendEmailForVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    private IEnumerator SendEmailForVerificationAsync()
    {
        if(user!=null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();

            yield return new WaitUntil(()=> sendEmailTask.IsCompleted);

            if(sendEmailTask.Exception !=null)
            {
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string errorMessage = "Unknown Error: Please try again later";

                switch (error)
                {
                    case AuthError.Cancelled:
                    errorMessage = "Email Verification is Cancelled";
                    break;
                    case AuthError.TooManyRequests:
                    errorMessage = "Too Many Request";
                    break;
                    case AuthError.InvalidRecipientEmail:
                    errorMessage = "Email is invalid";
                    break;
                }

                UIManager.Instance.ShowVerificationResponse(false, user.Email, errorMessage);
            }
            else
            {
                Debug.Log("Email has been sent sucessfully");
                UIManager.Instance.ShowVerificationResponse(true,user.Email,null);

            }
        }
    }
}