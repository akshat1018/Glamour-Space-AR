using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button backButton; // Reference to the back button
    public int sceneIndex = 3; // Scene index in Build Settings

    void Start()
    {
        // Assign button click event
        backButton.onClick.AddListener(GoBackToScene);
    }

    void GoBackToScene()
    {
        Debug.Log($"Returning to Scene Index {sceneIndex}...");
        SceneManager.LoadScene(sceneIndex);
    }
}
