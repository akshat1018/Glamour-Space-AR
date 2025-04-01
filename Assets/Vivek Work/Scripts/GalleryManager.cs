using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GalleryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect screenshotScrollView;
    [SerializeField] private RectTransform screenshotContentPanel;
    [SerializeField] private GameObject screenshotPrefab;
    [SerializeField] private RectTransform galleryUI;
    [SerializeField] private Button deleteButton;

    [Header("Enlarged Image Settings")]
    [SerializeField] private GameObject enlargedImagePrefab;
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Gallery Controls")]
    [SerializeField] private Button galleryToggleButton;
    [SerializeField] private Button galleryBackButton;

    [Header("Display Settings")]
    [SerializeField] private int thumbnailSize = 256;
    [SerializeField] private Color selectionColor = Color.cyan;
    [SerializeField] private Vector2 selectionOutlineSize = new Vector2(4, -3);
    private const int ENLARGED_WIDTH = 1000;
    private const int ENLARGED_HEIGHT = 1800;

    [Header("Interaction Settings")]
    [SerializeField] private float longPressDuration = 0.5f;

    [Header("Prefab Settings")]
    [SerializeField] private string thumbnailChildName = "ThumbnailImage";

    private List<Texture2D> loadedTextures = new List<Texture2D>();
    private List<string> loadedPaths = new List<string>();
    private List<GameObject> thumbnailButtons = new List<GameObject>();
    private List<GameObject> selectedThumbnails = new List<GameObject>();
    private GameObject currentEnlargedImage;
    private GameObject currentlyEnlargedThumbnail;
    private bool isAnimating = false;
    private bool isSelectionMode = false;
    private Coroutine longPressCoroutine;

    private void Start()
    {
        InitializeUI();
        LoadScreenshots();
    }

    private void InitializeUI()
    {
        galleryToggleButton.onClick.AddListener(ToggleGalleryUI);
        galleryBackButton.onClick.AddListener(OnBackButtonClicked);
        deleteButton.onClick.AddListener(DeleteSelected);

        deleteButton.gameObject.SetActive(false);
        galleryUI.localScale = Vector3.zero;
    }

    private void LoadScreenshots()
    {
        List<string> screenshotPaths = ScreenshotManager.GetScreenshotPaths();
        loadedPaths = new List<string>(screenshotPaths);
        StartCoroutine(LoadScreenshotsCoroutine(screenshotPaths));
    }

    private IEnumerator LoadScreenshotsCoroutine(List<string> paths)
    {
        foreach (string path in paths)
        {
            Texture2D texture = LoadTextureFromFile(path);
            if (texture != null)
            {
                loadedTextures.Add(texture);
                CreateThumbnailButton(texture, path);
                yield return null;
            }
        }
    }

    private Texture2D LoadTextureFromFile(string path)
    {
        try
        {
            if (!System.IO.File.Exists(path)) return null;

            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(fileData)) return null;
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading texture: {e.Message}");
            return null;
        }
    }

    private void CreateThumbnailButton(Texture2D texture, string path)
    {
        GameObject thumbnailObj = Instantiate(screenshotPrefab, screenshotContentPanel);
        thumbnailButtons.Add(thumbnailObj);

        // Set thumbnail image
        Transform thumbnailChild = thumbnailObj.transform.Find(thumbnailChildName);
        if (thumbnailChild != null)
        {
            Image thumbnailImage = thumbnailChild.GetComponent<Image>();
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = CreateThumbnailSprite(texture);
            }
        }

        // Setup event triggers
        EventTrigger trigger = thumbnailObj.GetComponent<EventTrigger>() ?? thumbnailObj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        // Pointer down - start long press detection
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => {
            longPressCoroutine = StartCoroutine(LongPressDetection(thumbnailObj));
        });
        trigger.triggers.Add(pointerDownEntry);

        // Pointer up - cancel long press
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => {
            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                longPressCoroutine = null;
            }
        });
        trigger.triggers.Add(pointerUpEntry);

        // Click - handle normal tap
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => {
            if (!isSelectionMode)
            {
                currentlyEnlargedThumbnail = thumbnailObj;
                ShowEnlargedImage(texture);
            }
        });
        trigger.triggers.Add(clickEntry);
    }

    private IEnumerator LongPressDetection(GameObject thumbnailObj)
    {
        float pressTime = Time.time;

        while (Time.time - pressTime < longPressDuration)
        {
            yield return null;
        }

        OnThumbnailLongPress(thumbnailObj);
        longPressCoroutine = null;
    }

    private void OnThumbnailLongPress(GameObject thumbnailObj)
    {
        if (!isSelectionMode)
        {
            isSelectionMode = true;
            deleteButton.gameObject.SetActive(true);
        }
        ToggleSelection(thumbnailObj);
    }

    private void ToggleSelection(GameObject thumbnailObj)
    {
        if (selectedThumbnails.Contains(thumbnailObj))
        {
            RemoveSelectionOutline(thumbnailObj);
            selectedThumbnails.Remove(thumbnailObj);
        }
        else
        {
            AddSelectionOutline(thumbnailObj);
            selectedThumbnails.Add(thumbnailObj);
        }

        if (selectedThumbnails.Count == 0)
        {
            isSelectionMode = false;
            deleteButton.gameObject.SetActive(false);
        }
    }

    private void AddSelectionOutline(GameObject thumbnailObj)
    {
        Outline outline = thumbnailObj.GetComponent<Outline>() ?? thumbnailObj.AddComponent<Outline>();
        outline.effectColor = selectionColor;
        outline.effectDistance = selectionOutlineSize;
        outline.enabled = true;
    }

    private void RemoveSelectionOutline(GameObject thumbnailObj)
    {
        Outline outline = thumbnailObj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    private void DeleteSelected()
    {
        StartCoroutine(DeleteSelectedCoroutine());
    }

    private IEnumerator DeleteSelectedCoroutine()
    {
        // Create a copy of the list to avoid modification during iteration
        List<GameObject> toDelete = new List<GameObject>(selectedThumbnails);

        foreach (GameObject thumbnail in toDelete)
        {
            if (thumbnail == null) continue;

            int index = thumbnailButtons.IndexOf(thumbnail);
            if (index >= 0 && index < loadedPaths.Count)
            {
                // Delete file
                if (System.IO.File.Exists(loadedPaths[index]))
                {
                    System.IO.File.Delete(loadedPaths[index]);
                }

                // Remove from lists
                if (index < loadedTextures.Count)
                {
                    Destroy(loadedTextures[index]);
                    loadedTextures.RemoveAt(index);
                }
                loadedPaths.RemoveAt(index);
            }

            // Remove from scene and tracking lists
            thumbnailButtons.Remove(thumbnail);
            selectedThumbnails.Remove(thumbnail);
            Destroy(thumbnail);

            yield return null;
        }

        // Rebuild layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(screenshotContentPanel);

        // Clear selection
        ClearSelection();
    }


    private Sprite CreateThumbnailSprite(Texture2D source)
    {
        Texture2D thumbnail = CreateThumbnailTexture(source);
        return Sprite.Create(thumbnail, new Rect(0, 0, thumbnailSize, thumbnailSize), Vector2.one * 0.5f);
    }

    private Texture2D CreateThumbnailTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(thumbnailSize, thumbnailSize);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D thumbnail = new Texture2D(thumbnailSize, thumbnailSize);
        thumbnail.ReadPixels(new Rect(0, 0, thumbnailSize, thumbnailSize), 0, 0);
        thumbnail.Apply();

        RenderTexture.ReleaseTemporary(rt);
        return thumbnail;
    }

    private void ShowEnlargedImage(Texture2D texture)
    {
        if (isAnimating || isSelectionMode) return;

        InitializeEnlargedImage();
        SetEnlargedImage(texture);
        AnimateEnlargedImage(true);

        // Enable delete button for this specific image
        deleteButton.gameObject.SetActive(true);
    }

    private void InitializeEnlargedImage()
    {
        if (currentEnlargedImage == null)
        {
            currentEnlargedImage = Instantiate(enlargedImagePrefab, galleryUI);
            ConfigureEnlargedImageRect();
            SetupCloseButton();
        }
    }

    private void ConfigureEnlargedImageRect()
    {
        RectTransform rt = currentEnlargedImage.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(ENLARGED_WIDTH, ENLARGED_HEIGHT);
        rt.localScale = Vector3.zero;
    }

    private void SetupCloseButton()
    {
        Button closeButton = currentEnlargedImage.GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideEnlargedImage);
        }
    }

    private void SetEnlargedImage(Texture2D texture)
    {
        Image img = currentEnlargedImage.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f);
        }
    }

    private void AnimateEnlargedImage(bool show)
    {
        isAnimating = true;
        currentEnlargedImage.transform.DOScale(show ? Vector3.one : Vector3.zero, animationDuration)
            .SetEase(show ? Ease.OutBack : Ease.InBack)
            .OnComplete(() => {
                isAnimating = false;
                if (!show) DestroyEnlargedImage();
            });
    }

    private void DestroyEnlargedImage()
    {
        Destroy(currentEnlargedImage);
        currentEnlargedImage = null;
        currentlyEnlargedThumbnail = null;
        deleteButton.gameObject.SetActive(false);
    }

    private void HideEnlargedImage()
    {
        if (currentEnlargedImage == null || isAnimating) return;
        AnimateEnlargedImage(false);
    }

    private void ToggleGalleryUI()
    {
        if (isAnimating) return;

        if (galleryUI.localScale == Vector3.zero)
        {
            ShowGalleryUI();
        }
        else
        {
            HideGalleryUI();
        }
    }

    private void ShowGalleryUI()
    {
        isAnimating = true;
        galleryUI.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => isAnimating = false);
    }

    private void HideGalleryUI()
    {
        isAnimating = true;
        galleryUI.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => isAnimating = false);
    }

    private void OnBackButtonClicked()
    {
        if (isSelectionMode)
        {
            ClearSelection();
        }
        else if (currentEnlargedImage != null)
        {
            HideEnlargedImage();
        }
        else
        {
            HideGalleryUI();
        }
    }

    private void ClearSelection()
    {
        // Create a new list to avoid modifying the collection while iterating
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject thumbnail in selectedThumbnails)
        {
            if (thumbnail != null) // Check if the object still exists
            {
                RemoveSelectionOutline(thumbnail);
            }
            toRemove.Add(thumbnail);
        }

        // Remove all items (including null ones) from the selection
        foreach (var item in toRemove)
        {
            selectedThumbnails.Remove(item);
        }

        isSelectionMode = false;
        deleteButton.gameObject.SetActive(false);
    }

   
    private void OnDestroy()
    {
        ClearLoadedTextures();
    }

    private void ClearLoadedTextures()
    {
        foreach (Texture2D texture in loadedTextures)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
        loadedTextures.Clear();
    }
}