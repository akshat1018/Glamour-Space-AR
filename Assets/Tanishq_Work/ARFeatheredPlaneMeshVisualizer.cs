using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ScrollRect))]
public class ARPlaneMaterialController : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Material[] planeMaterials;

    [Header("UI References")]
    [SerializeField] private Scrollbar materialScrollbar;
    [SerializeField] private Button[] materialButtons;
    [SerializeField] private Image[] buttonPreviews;

    [Header("Settings")]
    [SerializeField] private float scrollSensitivity = 0.1f;
    [SerializeField] private bool snapToButtons = true;

    private ScrollRect scrollRect;
    private int currentMaterialIndex = 0;
    private bool isDragging = false;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        InitializeReferences();
    }

    private void Start()
    {
        SetupScrollbar();
        SetupButtons();
        UpdateButtonPreviews();
        SnapToButton(0);
    }

    private void InitializeReferences()
    {
        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();
    }

    private void SetupScrollbar()
    {
        if (materialScrollbar != null)
        {
            materialScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
            materialScrollbar.numberOfSteps = planeMaterials.Length;
        }
    }

    private void SetupButtons()
    {
        if (materialButtons != null && materialButtons.Length > 0)
        {
            for (int i = 0; i < materialButtons.Length; i++)
            {
                int index = i;
                materialButtons[i].onClick.AddListener(() => OnMaterialButtonClicked(index));
            }
        }
    }

    private void UpdateButtonPreviews()
    {
        if (buttonPreviews == null || planeMaterials == null) return;

        for (int i = 0; i < buttonPreviews.Length && i < planeMaterials.Length; i++)
        {
            if (planeMaterials[i] != null && planeMaterials[i].mainTexture != null)
            {
                buttonPreviews[i].sprite = Sprite.Create(
                    (Texture2D)planeMaterials[i].mainTexture,
                    new Rect(0, 0, planeMaterials[i].mainTexture.width, planeMaterials[i].mainTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }
    }

    private void Update()
    {
        if (!isDragging && snapToButtons && scrollRect.velocity.magnitude < 200f)
        {
            SnapToNearestButton();
        }
    }

    public void OnBeginDrag() => isDragging = true;
    public void OnEndDrag() => isDragging = false;

    private void SnapToNearestButton()
    {
        if (materialButtons.Length == 0) return;
        float normalizedPos = scrollRect.horizontalNormalizedPosition;
        int closestIndex = Mathf.RoundToInt(normalizedPos * (materialButtons.Length - 1));
        SnapToButton(closestIndex);
    }

    private void SnapToButton(int index)
    {
        index = Mathf.Clamp(index, 0, materialButtons.Length - 1);
        scrollRect.horizontalNormalizedPosition = (float)index / (materialButtons.Length - 1);
        currentMaterialIndex = index;
        UpdatePlaneMaterial();
    }

    private void OnScrollbarValueChanged(float value)
    {
        if (isDragging) return;
        int materialIndex = Mathf.RoundToInt(value * (planeMaterials.Length - 1));
        if (materialIndex != currentMaterialIndex)
        {
            currentMaterialIndex = materialIndex;
            UpdatePlaneMaterial();
        }
    }

    private void OnMaterialButtonClicked(int materialIndex)
    {
        currentMaterialIndex = materialIndex;
        SnapToButton(materialIndex);
    }

    private void UpdatePlaneMaterial()
    {
        if (planeManager == null || currentMaterialIndex >= planeMaterials.Length) return;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.gameObject.activeInHierarchy)
            {
                MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = planeMaterials[currentMaterialIndex];
                }
            }
        }
    }

    public void NextMaterial()
    {
        int nextIndex = (currentMaterialIndex + 1) % planeMaterials.Length;
        SnapToButton(nextIndex);
    }

    public void PreviousMaterial()
    {
        int prevIndex = (currentMaterialIndex - 1 + planeMaterials.Length) % planeMaterials.Length;
        SnapToButton(prevIndex);
    }
}