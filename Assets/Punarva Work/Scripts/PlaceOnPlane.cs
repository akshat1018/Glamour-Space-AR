using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlaceOnPlane : MonoBehaviour
{
    public ObjectDatabase objectDatabase;
    public GameObject contentUIPrefab;
    public Transform scrollbarContent;
    private GameObject MyObject;
    public GameObject placementIndicator;
    public ARRaycastManager RaycastManager;
    public float scaleDuration = 1.0f;
    public Vector3 initialScale = Vector3.zero;
    public Vector3 finalScale = Vector3.one;
    public Button moveButton;
    public Button deleteButton;
    public Camera arCamera;
    public TMP_Text costText;

    private List<GameObject> placedObjects = new List<GameObject>();
    private Dictionary<GameObject, float> objectCostMap = new Dictionary<GameObject, float>();
    private float totalCost = 0;
    private GameObject visualIndicator;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private float initialPinchDistance;
    private Vector3 initialObjectScale;
    private float initialAngle;
    private Quaternion initialRotation;
    private bool isResizingEnabled = true;
    private bool canPlaceObject = false;
    private bool isDraggingEnabled = false;
    private bool isDragging = false;
    private GameObject selectedPlacedObject;

    private void Awake()
    {
        PopulateScrollbarContent();
    }

    void Start()
    {
        RaycastManager = GetComponent<ARRaycastManager>();
        visualIndicator = Instantiate(placementIndicator);
        visualIndicator.SetActive(false);
        MyObject = null;

        if (moveButton != null)
        {
            moveButton.onClick.AddListener(ToggleDragging);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelectedObject);
        }
        UpdateCostUI();
    }

    void Update()
    {
        if (!enabled) return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        if (IsTouchOverUI(touch.position.ReadValue()))
            return;

        if (canPlaceObject && touch.press.isPressed && placementPoseIsValid && MyObject != null &&
            touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            PlaceObject();
        }

        if (Touchscreen.current.touches.Count >= 2 && placedObjects.Count > 0)
        {
            HandlePinchAndRotation();
        }

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            SelectPlacedObject(touch.position.ReadValue());
        }

        if (isDraggingEnabled && isDragging)
        {
            DragObject();
        }
    }

    public void SetPlacementMode(bool active)
    {
        enabled = active;
        visualIndicator.SetActive(active && canPlaceObject);

        if (!active)
        {
            DeselectObject();
        }
    }

    public void DeselectObject()
    {
        MyObject = null;
        canPlaceObject = false;
        selectedPlacedObject = null;
        isDragging = false;
    }

    private void PopulateScrollbarContent()
    {
        if (contentUIPrefab == null || scrollbarContent == null || objectDatabase == null)
        {
            Debug.LogError("Button prefab, scrollbar content, or object database is not assigned.");
            return;
        }

        for (int i = 0; i < objectDatabase.objectsList.Count; i++)
        {
            GameObject buttonObject = Instantiate(contentUIPrefab, scrollbarContent);
            Button button = buttonObject.GetComponent<Button>();
            Image buttonImage = buttonObject.GetComponent<Image>();
            ObjectData objectData = objectDatabase.objectsList[i];

            if (buttonImage != null && objectData.image != null)
            {
                buttonImage.sprite = objectData.image;
            }

            int objectIndex = i;
            button.onClick.AddListener(() => SelectObjectToPlace(objectIndex));
        }
    }

    private void PlaceObject()
    {
        GameObject placedObject = Instantiate(MyObject, placementPose.position, placementPose.rotation);
        StartCoroutine(LerpObjectScale(initialScale, finalScale, scaleDuration, placedObject));
        placedObjects.Add(placedObject);

        float objectCost = GetObjectCost(MyObject);
        objectCostMap.Add(placedObject, objectCost);
        totalCost += objectCost;
        UpdateCostUI();

        canPlaceObject = false;
    }

    private void UpdateCostUI()
    {
        costText.text = "Total Cost: \nRs " + totalCost.ToString("F2");
    }

    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();

        placementPoseIsValid = RaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid && canPlaceObject)
        {
            visualIndicator.SetActive(true);
            visualIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            visualIndicator.SetActive(false);
        }
    }

    private IEnumerator LerpObjectScale(Vector3 a, Vector3 b, float time, GameObject lerpObject)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            lerpObject.transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
    }

    private void HandlePinchAndRotation()
    {
        var touch1 = Touchscreen.current.touches[0];
        var touch2 = Touchscreen.current.touches[1];

        if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began || touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            initialPinchDistance = Vector2.Distance(touch1.position.ReadValue(), touch2.position.ReadValue());
            initialObjectScale = selectedPlacedObject != null ? selectedPlacedObject.transform.localScale : Vector3.one;
            initialAngle = Vector2.SignedAngle(touch1.position.ReadValue() - touch2.position.ReadValue(), Vector2.right);
            initialRotation = selectedPlacedObject != null ? selectedPlacedObject.transform.rotation : Quaternion.identity;
        }
        else if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            float currentPinchDistance = Vector2.Distance(touch1.position.ReadValue(), touch2.position.ReadValue());
            if (Mathf.Approximately(initialPinchDistance, 0))
                return;

            if (isResizingEnabled && selectedPlacedObject != null)
            {
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                selectedPlacedObject.transform.localScale = initialObjectScale * scaleFactor;
            }

            float currentAngle = Vector2.SignedAngle(touch1.position.ReadValue() - touch2.position.ReadValue(), Vector2.right);
            float angleDelta = currentAngle - initialAngle;
            selectedPlacedObject.transform.rotation = initialRotation * Quaternion.Euler(0, angleDelta, 0);
        }
    }

    private void ToggleDragging()
    {
        isDraggingEnabled = !isDraggingEnabled;
        if (isDraggingEnabled && selectedPlacedObject != null)
        {
            StartDragging();
        }
        else
        {
            StopDragging();
        }
    }

    public void StartDragging()
    {
        if (selectedPlacedObject != null)
        {
            isDragging = true;
        }
    }

    public void StopDragging()
    {
        isDragging = false;
    }

    private void DragObject()
    {
        if (selectedPlacedObject == null) return;

        var touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (RaycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            selectedPlacedObject.transform.position = hits[0].pose.position;
        }
    }

    private void SelectPlacedObject(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (placedObjects.Contains(hit.collider.gameObject))
            {
                selectedPlacedObject = hit.collider.gameObject;
            }
        }
    }

    private void DeleteSelectedObject()
    {
        if (selectedPlacedObject != null)
        {
            if (objectCostMap.TryGetValue(selectedPlacedObject, out float objectCost))
            {
                totalCost -= objectCost;
                objectCostMap.Remove(selectedPlacedObject);
            }

            placedObjects.Remove(selectedPlacedObject);
            Destroy(selectedPlacedObject);
            selectedPlacedObject = null;

            UpdateCostUI();
        }
    }

    public void SelectObjectToPlace(int objectIndex)
    {
        if (objectIndex >= 0 && objectIndex < objectDatabase.objectsList.Count)
        {
            MyObject = objectDatabase.objectsList[objectIndex].objectToPlace;
            canPlaceObject = true;
            visualIndicator.SetActive(true);
        }
    }

    private float GetObjectCost(GameObject objectToPlace)
    {
        foreach (var objectData in objectDatabase.objectsList)
        {
            if (objectData.objectToPlace == objectToPlace)
            {
                return objectData.price;
            }
        }
        return 0;
    }
}