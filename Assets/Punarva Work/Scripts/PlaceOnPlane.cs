using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems; // For UI touch checking
using TMPro;


public class PlaceOnPlane : MonoBehaviour
{
    public GameObject[] objectsToPlace; // Array to hold the objects to place
    public float[] objectCosts; // Array to hold the cost of each object
    private GameObject MyObject; // The currently selected object
    public GameObject placementIndicator;
    public ARRaycastManager RaycastManager;
    public float scaleDuration = 1.0f;
    public Vector3 initialScale = Vector3.zero;
    public Vector3 finalScale = Vector3.one;
    

    public Button moveButton; // Reference to the TMP Button for toggling drag
    public Button deleteButton; // Reference to the button for deleting the selected object
    public Camera arCamera; // AR camera for raycasting
    public TMP_Text costText; // TMPro element to display the total cost

    private List<GameObject> placedObjects = new List<GameObject>(); // List to store placed objects
    private Dictionary<GameObject, float> objectCostMap = new Dictionary<GameObject, float>(); // Map to track costs of placed objects
    private float totalCost = 0; // Total cost of placed objects
    private GameObject visualIndicator;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private float initialPinchDistance;
    private Vector3 initialObjectScale;
    private float initialAngle;
    private Quaternion initialRotation;
    private bool isResizingEnabled = true;
    private bool canPlaceObject = false; // Track if the object can be placed
    private bool isDraggingEnabled = false; // Dragging enabled/disabled state
    private bool isDragging = false; // Dragging state
    private GameObject selectedPlacedObject; // Object being dragged
   

    void Start()
    {
        RaycastManager = GetComponent<ARRaycastManager>();
        visualIndicator = Instantiate(placementIndicator);
        MyObject = null; // No object selected initially

        // Initialize dragging toggle button if assigned
        if (moveButton != null)
        {
            moveButton.onClick.AddListener(ToggleDragging); // Assign the toggle functionality to the button
        }

        // Initialize delete button if assigned
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelectedObject); // Assign the delete functionality to the button
        }
        UpdateCostUI();

    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        // Skip processing if the touch is over a UI element
        if (IsTouchOverUI(touch.position.ReadValue()))
            return;

        // Handle object placement
        if (canPlaceObject && touch.press.isPressed && placementPoseIsValid && MyObject != null &&
            touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            // Instantiate the object only once after selection
            GameObject placedObject = Instantiate(MyObject, placementPose.position, placementPose.rotation);
            StartCoroutine(LerpObjectScale(initialScale, finalScale, scaleDuration, placedObject));
            placedObjects.Add(placedObject); // Store placed object in the list

            // Add the cost of the placed object
            float objectCost = objectCosts[System.Array.IndexOf(objectsToPlace, MyObject)];
            objectCostMap.Add(placedObject, objectCost);
            totalCost += objectCost;
            UpdateCostUI();

            
            canPlaceObject = false; // Disable further placement of the same object
        }

        // Handle object resizing and rotation with pinch gesture
        if (Touchscreen.current.touches.Count >= 2 && placedObjects.Count > 0)
        {
            HandlePinchAndRotation();
        }

        // Handle touch for selecting an object
        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            SelectPlacedObject(touch.position.ReadValue());
        }

        // Handle dragging of the selected object
        if (isDraggingEnabled && isDragging)
        {
            DragObject();
        }
    }
    private void UpdateCostUI()
    {
        costText.text = "Total Cost: \nRs " + totalCost.ToString("F2"); // Update the TMPro UI text
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
        isDraggingEnabled = !isDraggingEnabled; // Toggle the dragging state
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
            // Move the object to the new position
            selectedPlacedObject.transform.position = hits[0].pose.position;
        }
    }

    // Selects the object the user taps on
    private void SelectPlacedObject(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object is one of the placed objects
            if (placedObjects.Contains(hit.collider.gameObject))
            {
                selectedPlacedObject = hit.collider.gameObject; // Select the object that was touched
            }
        }
    }

    // Deletes the currently selected object
    private void DeleteSelectedObject()
    {
        if (selectedPlacedObject != null)
        {
            // Subtract the cost of the selected object from the total
            if (objectCostMap.TryGetValue(selectedPlacedObject, out float objectCost))
            {
                totalCost -= objectCost;
                objectCostMap.Remove(selectedPlacedObject);
            }

            placedObjects.Remove(selectedPlacedObject);
            Destroy(selectedPlacedObject);
            selectedPlacedObject = null;

            UpdateCostUI(); // Update the total cost after deletion
        }
    }


    // Called when a new object is selected from the UI
    public void SelectObjectToPlace(int objectIndex)
    {
        if (objectIndex >= 0 && objectIndex < objectsToPlace.Length)
        {
            MyObject = objectsToPlace[objectIndex];
            canPlaceObject = true; // Allow placing the newly selected object
        }
    }
}
