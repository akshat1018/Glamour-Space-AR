using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BudgetUIToggle : MonoBehaviour
{
    public GameObject budgetUI; // Reference to the budgetUI GameObject
    public Button toggleButton; // Button to toggle the budgetUI
    public Button backButton;   // Back button to hide the budgetUI

    // Reference to the BudgetUIManager to clear results
    public BudgetUIManager budgetUIManager;

    private bool isAnimating = false; // Flag to check if an animation is in progress

    void Start()
    {
        // Null checks for budgetUI
        if (budgetUI == null)
        {
           // Debug.LogWarning("⚠️ budgetUI is not assigned in Inspector!");
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

        // Null checks for budgetUIManager
        if (budgetUIManager == null)
        {
           // Debug.LogWarning("⚠️ budgetUIManager is not assigned in Inspector!");
            return;
        }

        // Assign event listeners safely
        toggleButton.onClick.AddListener(ToggleBudgetUI);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Ensure budgetUI starts as inactive and scaled down
        budgetUI.transform.localScale = Vector3.zero;
        budgetUI.SetActive(false);
    }

    void ToggleBudgetUI()
    {
        if (isAnimating) return; // Prevent multiple clicks during animation

        if (budgetUI == null)
        {
           // Debug.LogError("BudgetUI is not assigned!");
            return;
        }

        // Toggle the budgetUI visibility
        if (budgetUI.activeSelf)
        {
            StartCoroutine(HideBudgetUI());
        }
        else
        {
            StartCoroutine(ShowBudgetUI());
        }
    }

    private System.Collections.IEnumerator ShowBudgetUI()
    {
        isAnimating = true;
        budgetUIManager.budgetInput.text = "";
        budgetUI.SetActive(true);
        budgetUI.transform.localScale = Vector3.zero; // Start from zero scale
        budgetUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
               // Debug.Log("✅ Budget UI Activated and Scaled Up");
                isAnimating = false;
            });

        yield return null;
    }

    private System.Collections.IEnumerator HideBudgetUI()
    {
        isAnimating = true;
        budgetUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                budgetUI.SetActive(false);
               // Debug.Log("BudgetUI hidden.");
                isAnimating = false;
            });

        yield return null;
    }

    public void OnBackButtonClicked()
    {
        if (isAnimating) return; // Prevent multiple clicks during animation

        if (budgetUI == null)
        {
            //Debug.LogError("BudgetUI is not assigned!");
            return;
        }

        StartCoroutine(HideBudgetUIAndClearResults());
    }

    private System.Collections.IEnumerator HideBudgetUIAndClearResults()
    {
        isAnimating = true;
        budgetUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // Clear results when the animation is complete
                if (budgetUIManager != null)
                {
                    budgetUIManager.ClearResults();
                }
                budgetUI.SetActive(false);
               // Debug.Log("BudgetUI hidden and results cleared.");
                isAnimating = false;
            });

        yield return null;
    }
}