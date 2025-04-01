using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class BudgetUIToggle : MonoBehaviour
{
    public GameObject budgetUI;
    public Button toggleButton;
    public Button backButton;
    public BudgetUIManager budgetUIManager;

    private bool isAnimating = false;

    void Start()
    {
        if (budgetUI == null || toggleButton == null || backButton == null || budgetUIManager == null)
        {
            Debug.LogWarning("Some references are not assigned in Inspector!");
            return;
        }

        toggleButton.onClick.AddListener(ToggleBudgetUI);
        backButton.onClick.AddListener(OnBackButtonClicked);

        budgetUI.transform.localScale = Vector3.zero;
        budgetUI.SetActive(false);
    }

    void ToggleBudgetUI()
    {
        if (isAnimating) return;

        if (budgetUI.activeSelf)
        {
            StartCoroutine(HideBudgetUIAndReset());
        }
        else
        {
            StartCoroutine(ShowBudgetUI());
        }
    }

    private IEnumerator ShowBudgetUI()
    {
        isAnimating = true;
        budgetUI.SetActive(true);
        budgetUI.transform.localScale = Vector3.zero;
        budgetUI.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => isAnimating = false);
        yield return null;
    }

    private IEnumerator HideBudgetUIAndReset()
    {
        isAnimating = true;
        budgetUI.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                budgetUIManager.ResetUI();
                budgetUI.SetActive(false);
                isAnimating = false;
            });
        yield return null;
    }

    public void OnBackButtonClicked()
    {
        if (isAnimating) return;
        StartCoroutine(HideBudgetUIAndReset());
    }
}