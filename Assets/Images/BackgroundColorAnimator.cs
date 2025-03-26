using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class BackgroundColorAnimator : MonoBehaviour
{
    [Header("Settings")]
    public float colorChangeDuration = 0.3f; // Duration for each random color change
    public float randomColorsDuration = 2f;  // Total time for random colors phase
    public float returnDuration = 1f;        // Duration to return to original color

    private Image backgroundImage;
    private Color originalColor;
    private Coroutine colorAnimationRoutine;

    void Awake()
    {
        backgroundImage = GetComponent<Image>();
        originalColor = backgroundImage.color;
    }

    void OnEnable()
    {
        StartColorAnimation();
    }

    void OnDisable()
    {
        StopColorAnimation();
    }

    public void StartColorAnimation()
    {
        if (colorAnimationRoutine != null)
            StopCoroutine(colorAnimationRoutine);
        
        colorAnimationRoutine = StartCoroutine(ColorAnimationSequence());
    }

    public void StopColorAnimation()
    {
        if (colorAnimationRoutine != null)
        {
            StopCoroutine(colorAnimationRoutine);
            colorAnimationRoutine = null;
        }
        
        // Kill any active tweens
        DOTween.Kill(backgroundImage);
        backgroundImage.color = originalColor;
    }

    private IEnumerator ColorAnimationSequence()
    {
        // Phase 1: Random colors for 2 seconds
        float timer = 0f;
        while (timer < randomColorsDuration)
        {
            Color randomColor = new Color(
                Random.value, 
                Random.value, 
                Random.value,
                originalColor.a // Maintain original alpha
            );

            backgroundImage.DOColor(randomColor, colorChangeDuration)
                .SetEase(Ease.InOutQuad);

            timer += colorChangeDuration;
            yield return new WaitForSeconds(colorChangeDuration);
        }

        // Phase 2: Return to original color
        backgroundImage.DOColor(originalColor, returnDuration)
            .SetEase(Ease.InOutQuad);
    }

    // Call this to restart the animation
    public void RestartAnimation()
    {
        StopColorAnimation();
        StartColorAnimation();
    }
}