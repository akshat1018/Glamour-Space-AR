using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems; // For UI touch checking
using NativeFilePickerNamespace;
using Dummiesman; // For Runtime OBJ Importer
using System.IO;  // For file handling


public class ModelPicker : MonoBehaviour
{
    public Button addButton;
    public Transform inventoryPanel; // Where buttons will be added dynamically
    public GameObject buttonPrefab;  // Button prefab to represent objects

    private List<string> modelPaths = new List<string>(); // Store file paths for inventory

    void Start()
    {
        addButton.onClick.AddListener(OpenFileBrowser);
    }

    void OpenFileBrowser()
    {
        // Check if the permission is granted first
        NativeFilePicker.Permission permission = NativeFilePicker.CheckPermission();

        if (permission == NativeFilePicker.Permission.Granted)
        {
            // Permission is granted, proceed to open file picker
            NativeFilePicker.PickFile((path) =>
            {
                if (path != null)
                {
                    if (path.EndsWith(".obj") || path.EndsWith(".fbx"))
                    {
                        // Add the selected file to the inventory
                        modelPaths.Add(path);
                        Debug.Log("Selected Model: " + path);

                        // Dynamically create a button for the selected model
                        AddModelToInventory(path);
                    }
                    else
                    {
                        Debug.LogError("Selected file is not a valid 3D model");
                    }
                }
            }, new string[] { "obj", "fbx" });
        }
        else if (permission == NativeFilePicker.Permission.Denied)
        {
            // If the permission is denied, request permission
            permission = NativeFilePicker.RequestPermission();

            if (permission == NativeFilePicker.Permission.Granted)
            {
                // Open file picker again after permission is granted
                OpenFileBrowser();
            }
            else
            {
                Debug.LogError("Permission to access storage is denied.");
            }
        }
    }


    void AddModelToInventory(string path)
    {
        // Instantiate button prefab
        GameObject buttonObject = Instantiate(buttonPrefab, inventoryPanel);
        Button button = buttonObject.GetComponent<Button>();

        // Set button text and onClick event
        button.GetComponentInChildren<Text>().text = "Model " + modelPaths.Count;
        int index = modelPaths.Count - 1;
        button.onClick.AddListener(() => OnModelButtonClicked(index));

        // Load the model and display it when clicked (load at runtime)
        StartCoroutine(Load3DModel(path));
    }

    void OnModelButtonClicked(int index)
    {
        string path = modelPaths[index];
        Debug.Log("Selected model from inventory: " + path);
        // Here you can instantiate or preview the model in your AR scene
        StartCoroutine(Load3DModel(path));
    }



    IEnumerator Load3DModel(string path)
    {
        string fixedPath = "file://" + path;

        // Load the OBJ file using Runtime OBJ Importer
        GameObject loadedObject = null;

        if (path.EndsWith(".obj"))
        {
            // Open the file as a FileStream
            using (var objFileStream = new FileStream(path, FileMode.Open))
            {
                // Parse the OBJ file
                loadedObject = new OBJLoader().Load(objFileStream);

                if (loadedObject != null)
                {
                    Debug.Log("Model loaded successfully: " + path);
                    loadedObject.transform.localScale = Vector3.one; // Set default scale
                }
                else
                {
                    Debug.LogError("Failed to load OBJ model: " + path);
                }
            }
        }
        else
        {
            Debug.LogError("Only .obj format is supported for now.");
        }

        yield return null;
    }
}
