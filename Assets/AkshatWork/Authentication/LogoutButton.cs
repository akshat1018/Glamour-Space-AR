using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutButton : MonoBehaviour
{
   public void OnLogoutButtonClick()
{
    if(FirebaseAuthManager.Instance != null)
    {
        FirebaseAuthManager.Instance.Logout();
    }
    else
    {
        Debug.LogError("FirebaseAuthManager instance not found!");
        // Optionally reload the login scene
        SceneManager.LoadScene("1_User_Login");
    }
}
}
