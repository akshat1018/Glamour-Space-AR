using UnityEngine;
using UnityEngine.UI;

public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound; // Assign the click sound in the Inspector
    private AudioSource audioSource;

    private void Start()
    {
        // Ensure there is an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set the AudioSource settings
        audioSource.playOnAwake = false; // Prevent sound from playing on start
        audioSource.clip = clickSound;

        // Find all GameObjects tagged as "Button"
        GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("Button");
        foreach (GameObject buttonObject in buttonObjects)
        {
            Button buttonComponent = buttonObject.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(PlayClickSound);
            }
            else
            {
                Debug.LogWarning($"GameObject '{buttonObject.name}' tagged as 'Button' does not have a Button component.");
            }
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Click sound is not assigned in the Inspector.");
        }
    }
}
