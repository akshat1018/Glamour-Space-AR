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
        audioSource.playOnAwake = false;
        audioSource.clip = clickSound;

        // Automatically assign the PlayClickSound function to all buttons in the scene
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // Public function to play the click sound
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            Debug.Log("Sound Played: " + clickSound.name);
        }
        else
        {
            Debug.LogError("Click sound is not assigned in the Inspector.");
        }
    }
}