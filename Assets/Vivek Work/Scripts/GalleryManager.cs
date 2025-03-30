using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;

public class GalleryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect screenshotScrollView;
    [SerializeField] private RectTransform screenshotContentPanel;
    [SerializeField] private GameObject screenshotPrefab;
    [SerializeField] private RectTransform galleryUI;

    [Header("Enlarged Image Settings")]
    [SerializeField] private GameObject enlargedImagePrefab;
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Gallery Controls")]
    [SerializeField] private Button galleryToggleButton;
    [SerializeField] private Button galleryBackButton;

    [Header("Display Settings")]
    [SerializeField] private int thumbnailSize = 256;
    private const int ENLARGED_WIDTH = 1000;
    private const int ENLARGED_HEIGHT = 1800;

    private List<Texture2D> loadedTextures = new List<Texture2D>();
    private GameObject currentEnlargedImage;
    private bool isAnimating = false;

    private void Start()
    {
        InitializeUI();
        LoadScreenshots();
    }

    private void InitializeUI()
    {
        galleryToggleButton.onClick.AddListener(ToggleGalleryUI);
        galleryBackButton.onClick.AddListener(OnBackButtonClicked);
        galleryUI.localScale = Vector3.zero;
    }

    private void LoadScreenshots()
    {
        List<string> screenshotPaths = ScreenshotManager.GetScreenshotPaths();
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
                CreateThumbnailButton(texture);
                yield return null;
            }
        }
    }

    private Texture2D LoadTextureFromFile(string path)
    {
        if (!File.Exists(path)) return null;

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private void CreateThumbnailButton(Texture2D texture)
    {
        GameObject thumbnailObj = Instantiate(screenshotPrefab, screenshotContentPanel);
        Image thumbnailImage = thumbnailObj.GetComponentInChildren<Image>();

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = CreateThumbnailSprite(texture);
        }

        Button button = thumbnailObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ShowEnlargedImage(texture));
        }
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
        if (isAnimating) return;

        InitializeEnlargedImage();
        SetEnlargedImage(texture);
        AnimateEnlargedImage(true);
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
        if (currentEnlargedImage != null)
        {
            HideEnlargedImage();
        }
        else
        {
            HideGalleryUI();
        }
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