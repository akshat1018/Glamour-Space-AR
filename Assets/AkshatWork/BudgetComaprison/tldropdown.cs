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

    void Start()
    {
        // Null checks for budgetUI
        if (budgetUI == null)
        {
            Debug.LogWarning("⚠️ budgetUI is not assigned in Inspector!");
            return;
        }

        // Null checks for toggleButton
        if (toggleButton == null)
        {
            Debug.LogWarning("⚠️ toggleButton is not assigned in Inspector!");
            return;
        }

        // Null checks for backButton
        if (backButton == null)
        {
            Debug.LogWarning("⚠️ backButton is not assigned in Inspector!");
            return;
        }

        // Null checks for budgetUIManager
        if (budgetUIManager == null)
        {
            Debug.LogWarning("⚠️ budgetUIManager is not assigned in Inspector!");
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
        if (budgetUI == null)
        {
            Debug.LogError("BudgetUI is not assigned!");
            return;
        }

        // Toggle the budgetUI visibility
        if (budgetUI.activeSelf)
        {
            // Hide the budgetUI using scaling animation
            budgetUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    budgetUI.SetActive(false);
                    Debug.Log("BudgetUI hidden.");
                });
        }
        else
        {
            budgetUIManager.budgetInput.text="";
            // Show the budgetUI using scaling animation
            budgetUI.SetActive(true);
            budgetUI.transform.localScale = Vector3.zero; // Start from zero scale
            budgetUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    Debug.Log("✅ Budget UI Activated and Scaled Up");
                });
        }
    }

    public void OnBackButtonClicked()
    {
        if (budgetUI == null)
        {
            Debug.LogError("BudgetUI is not assigned!");
            return;
        }

        // Hide the budgetUI using scaling animation
        budgetUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // Clear results when the animation is complete
                if (budgetUIManager != null)
                {
                    budgetUIManager.ClearResults();
                }
                Debug.Log("BudgetUI hidden and results cleared.");
            });
    }
}