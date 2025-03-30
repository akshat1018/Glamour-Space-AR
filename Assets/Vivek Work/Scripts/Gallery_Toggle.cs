using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Gallery_Toggle : MonoBehaviour
{
    public GameObject galleryUI; // Reference to the galleryUI GameObject
    public Button toggleButton; // Button to toggle the galleryUI
    public Button backButton;   // Back button to hide the galleryUI

    // Reference to the galleryUIManager to clear results
    

    private bool isAnimating = false; // Flag to check if an animation is in progress

    void Start()
    {
        // Null checks for galleryUI
        if (galleryUI == null)
        {
           // Debug.LogWarning("⚠️ galleryUI is not assigned in Inspector!");
            return;
        }

        // Null checks for toggleButton
        if (toggleButton == null)
        {
           // Debug.LogWarning("⚠️ toggleButton is not assigned in Inspector!");
            return;
        }

        // Null checks for backButton
        if (backButton == null)
        {
            //Debug.LogWarning("⚠️ backButton is not assigned in Inspector!");
            return;
        }

        // Null checks for galleryUIManager
        

        // Assign event listeners safely
        toggleButton.onClick.AddListener(TogglegalleryUI);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Ensure galleryUI starts as inactive and scaled down
        galleryUI.transform.localScale = Vector3.zero;
        galleryUI.SetActive(false);
    }

    void TogglegalleryUI()
    {
        if (isAnimating) return; // Prevent multiple clicks during animation

        if (galleryUI == null)
        {
           // Debug.LogError("galleryUI is not assigned!");
            return;
        }

        // Toggle the galleryUI visibility
        if (galleryUI.activeSelf)
        {
            StartCoroutine(HidegalleryUI());
        }
        else
        {
            StartCoroutine(ShowgalleryUI());
        }
    }

    private System.Collections.IEnumerator ShowgalleryUI()
    {
        isAnimating = true;
        
        galleryUI.SetActive(true);
        galleryUI.transform.localScale = Vector3.zero; // Start from zero scale
        galleryUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
               // Debug.Log("✅ gallery UI Activated and Scaled Up");
                isAnimating = false;
            });

        yield return null;
    }

    private System.Collections.IEnumerator HidegalleryUI()
    {
        isAnimating = true;
        galleryUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                galleryUI.SetActive(false);
               // Debug.Log("galleryUI hidden.");
                isAnimating = false;
            });

        yield return null;
    }

    public void OnBackButtonClicked()
    {
        if (isAnimating) return; // Prevent multiple clicks during animation

        if (galleryUI == null)
        {
            //Debug.LogError("galleryUI is not assigned!");
            return;
        }

        StartCoroutine(HidegalleryUIAndClearResults());
    }

    private System.Collections.IEnumerator HidegalleryUIAndClearResults()
    {
        isAnimating = true;
        galleryUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // Clear results when the animation is complete
                
                galleryUI.SetActive(false);
               // Debug.Log("galleryUI hidden and results cleared.");
                isAnimating = false;
            });

        yield return null;
    }
}