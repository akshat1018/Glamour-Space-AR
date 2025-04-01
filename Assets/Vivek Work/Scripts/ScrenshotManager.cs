using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ScreenshotManager : MonoBehaviour
{
    [Header("UI References")]
    public Button captureButton;
    public string gallerySceneName = "3_Main Menu";
    public Canvas[] uiCanvasesToHide; // Assign all UI canvases you want to hide during screenshot

    [Header("Objects to Hide")]
    public GameObject[] objectsToHide; // Assign specific GameObjects/Prefabs to hide during screenshot

    [Header("Screenshot Settings")]
    public string screenshotFolder = "Screenshots";
    public string screenshotBaseName = "Screenshot";
    public ImageFormat imageFormat = ImageFormat.PNG;
    public bool includeTimestamp = true;
    public int thumbnailSize = 256;

    private string fullScreenshotPath;
    private static List<string> screenshotPaths = new List<string>();
    private List<bool> originalCanvasStates = new List<bool>();
    private List<bool> originalObjectStates = new List<bool>();

    public enum ImageFormat
    {
        PNG,
        JPG
    }

    private void Start()
    {
        // Set up button listener
        if (captureButton != null)
        {
            captureButton.onClick.AddListener(TakeScreenshot);
        }

        // Create screenshot directory if needed
        fullScreenshotPath = Path.Combine(Application.persistentDataPath, screenshotFolder);
        if (!Directory.Exists(fullScreenshotPath))
        {
            Directory.CreateDirectory(fullScreenshotPath);
        }

        // Load existing screenshots if not already loaded
        if (screenshotPaths.Count == 0)
        {
            LoadExistingScreenshotPaths();
        }
    }

    public void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoroutine());
    }

    private IEnumerator CaptureScreenshotCoroutine()
    {
        // Disable button during capture
        if (captureButton != null)
        {
            captureButton.interactable = false;
        }

        // Store and hide UI elements
        originalCanvasStates.Clear();
        foreach (Canvas canvas in uiCanvasesToHide)
        {
            if (canvas != null)
            {
                originalCanvasStates.Add(canvas.enabled);
                canvas.enabled = false;
            }
        }

        // Store and hide specific GameObjects
        originalObjectStates.Clear();
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                originalObjectStates.Add(obj.activeSelf);
                obj.SetActive(false);
            }
        }

        // Wait for one frame to ensure everything is fully hidden
        yield return null;

        // Wait for rendering to complete
        yield return new WaitForEndOfFrame();

        // Create texture from screen
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        // Generate filename
        string timestamp = includeTimestamp ? "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") : "";
        string extension = imageFormat == ImageFormat.PNG ? ".png" : ".jpg";
        string fileName = screenshotBaseName + timestamp + extension;
        string filePath = Path.Combine(fullScreenshotPath, fileName);

        // Save to file
        byte[] bytes = imageFormat == ImageFormat.PNG ?
            screenTexture.EncodeToPNG() : screenTexture.EncodeToJPG(85);
        File.WriteAllBytes(filePath, bytes);

        // Add to global list
        screenshotPaths.Add(filePath);

        // Restore UI elements
        for (int i = 0; i < uiCanvasesToHide.Length; i++)
        {
            if (uiCanvasesToHide[i] != null && i < originalCanvasStates.Count)
            {
                uiCanvasesToHide[i].enabled = originalCanvasStates[i];
            }
        }

        // Restore GameObjects
        for (int i = 0; i < objectsToHide.Length; i++)
        {
            if (objectsToHide[i] != null && i < originalObjectStates.Count)
            {
                objectsToHide[i].SetActive(originalObjectStates[i]);
            }
        }

        // Re-enable button
        if (captureButton != null)
        {
            captureButton.interactable = true;
        }

        // Clean up
        Destroy(screenTexture);
    }

    private void LoadExistingScreenshotPaths()
    {
        screenshotPaths.Clear();
        string[] files = Directory.GetFiles(fullScreenshotPath);
        foreach (string file in files)
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg"))
            {
                screenshotPaths.Add(file);
            }
        }
    }

    public static List<string> GetScreenshotPaths()
    {
        return new List<string>(screenshotPaths);
    }

    public void OpenGallery()
    {
        SceneManager.LoadScene(gallerySceneName);
    }
}