using UnityEngine;
using UnityEngine.UI;

public class OpenURL : MonoBehaviour
{
    public string url = "https://glamour-space.onrender.com/feedbacks";

    void Start()
    {
        // Get the Button component and add a listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenWebURL);
        }
    }

    public void OpenWebURL()
    {
        Application.OpenURL(url);
    }
}