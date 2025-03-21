using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Import Dotween namespace

public class ProfileOpener : MonoBehaviour
{
    public GameObject profileMenu;
    public CanvasGroup profileCanvasGroup; // Use CanvasGroup for fade effects
    public float fadeDuration = 0.5f; // Duration of the fade animation
    public float scaleDuration = 0.3f; // Duration of the scale animation

    private void Start()
    {
        // Ensure the profile menu is initially hidden
        profileMenu.SetActive(false);
        if (profileCanvasGroup != null)
        {
            profileCanvasGroup.alpha = 0; // Set alpha to 0 for fade-in effect
        }
    }

    public void OpenProfile()
    {
        // Activate the menu before animating
        profileMenu.SetActive(true);

        // Fade-in animation
        if (profileCanvasGroup != null)
        {
            profileCanvasGroup.DOFade(1, fadeDuration).SetEase(Ease.OutQuad);
        }

        // Scale animation (optional)
        profileMenu.transform.localScale = Vector3.zero;
        profileMenu.transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack);
    }

    public void CloseProfile()
    {
        // Fade-out animation
        if (profileCanvasGroup != null)
        {
            profileCanvasGroup.DOFade(0, fadeDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => profileMenu.SetActive(false)); // Deactivate after animation
        }

        // No scale animation during close
    }
}