using UnityEngine;
using TMPro;  // Import the TextMeshPro namespace

public class WelcomeText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text messageText;  // Use TMP_Text instead of Text

    // Start is called before the first frame update
    void Start()
    {
        ShowMessage();   
    }

    private void ShowMessage()
    {
        messageText.text = string.Format("Welcome, {0} to GlamourSpace AR", References.userName);
    }
}
