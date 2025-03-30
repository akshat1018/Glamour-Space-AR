using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GalleryManager : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect screenshotScrollView;
    public GameObject screenshotContentPanel;
    public GameObject screenshotPrefab; // Prefab with Image component

    [Header("Display Settings")]
    public int thumbnailSize = 256;

    private void Start()
    {
        LoadScreenshots();
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
            // Load the file
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // This auto-resizes the texture

            // Create thumbnail in scroll view
            AddScreenshotToScrollView(texture, Path.GetFileName(path));

            yield return null; // Spread loading over multiple frames
        }
    }

    private void AddScreenshotToScrollView(Texture2D texture, string filename)
    {
        if (screenshotPrefab == null || screenshotContentPanel == null)
        {
            Debug.LogWarning("Screenshot prefab or content panel not assigned!");
            return;
        }

        // Create thumbnail
        Texture2D thumbnail = CreateThumbnail(texture, thumbnailSize);

        // Create UI element
        GameObject screenshotUI = Instantiate(screenshotPrefab, screenshotContentPanel.transform);
        Image imageComponent = screenshotUI.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = Sprite.Create(thumbnail, 
                new Rect(0, 0, thumbnail.width, thumbnail.height), 
                new Vector2(0.5f, 0.5f));
        }

        // Optional: Add click handler to view full image
        Button button = screenshotUI.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ShowFullImage(texture));
        }

        // Clean up
        Destroy(texture);
    }

    private Texture2D CreateThumbnail(Texture2D source, int size)
    {
        int width, height;
        if (source.width > source.height)
        {
            width = size;
            height = Mathf.RoundToInt((float)source.height / source.width * size);
        }
        else
        {
            height = size;
            width = Mathf.RoundToInt((float)source.width / source.height * size);
        }

        // Create render texture
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        // Create new texture
        Texture2D thumbnail = new Texture2D(width, height, TextureFormat.RGB24, false);
        thumbnail.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        thumbnail.Apply();

        // Clean up
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return thumbnail;
    }

    private void ShowFullImage(Texture2D texture)
    {
        // Implement your full image display logic here
        Debug.Log("Displaying full image: " + texture.width + "x" + texture.height);
    }
}