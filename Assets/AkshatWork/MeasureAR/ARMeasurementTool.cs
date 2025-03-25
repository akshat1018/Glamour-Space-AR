using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ARMeasurementTool : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;
    public LineRenderer lineRenderer;

    [Header("Prefabs")]
    public TMP_Text measurementTextPrefab;
    public GameObject edgePointPrefab;
    public GameObject visualTrackerPrefab;

    [Header("UI References")]
    public Canvas worldSpaceCanvas;
    public Button resetButton;
    public Button previousButton;
    public TMP_Dropdown unitDropdown;
    public TMP_Dropdown areaDropdown;

    private List<Vector3> measurementPoints = new List<Vector3>();
    private List<GameObject> measurementPointInstances = new List<GameObject>();
    private List<TMP_Text> measurementTextInstances = new List<TMP_Text>();
    private GameObject currentTracker;
    private const float OverlapThreshold = 0.05f;
    private MeasurementUnit currentUnit = MeasurementUnit.Centimeters;
    private DisplayMode currentDisplayMode = DisplayMode.Area;

    // Input System
    private InputAction tapAction;
    private bool isInitialized = false;

    private enum MeasurementUnit
    {
        Centimeters,
        Meters,
        Inches,
        Feet
    }

    private enum DisplayMode
    {
        Area,
        Perimeter
    }

    private void Awake()
    {
        tapAction = new InputAction(binding: "<Pointer>/press");
        tapAction.performed += ctx => OnTap();
    }

    private void Start()
    {
        InitializeMeasurementSystem();
    }

    private void InitializeMeasurementSystem()
    {
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;

        resetButton.onClick.AddListener(ResetMeasurement);
        previousButton.onClick.AddListener(GoToPreviousPoint);
        previousButton.interactable = false;

        if (unitDropdown != null)
        {
            unitDropdown.onValueChanged.AddListener(OnUnitChanged);
            unitDropdown.value = (int)MeasurementUnit.Centimeters;
        }

        if (areaDropdown != null)
        {
            areaDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
            areaDropdown.value = (int)DisplayMode.Area;
        }

        arPlaneManager.planesChanged += OnPlanesChanged;
        tapAction.Enable();
        isInitialized = true;
    }

    private void GoToPreviousPoint()
    {
        if (measurementPoints.Count == 0 || measurementPointInstances.Count == 0)
            return;

        int lastIndex = measurementPoints.Count - 1;
        measurementPoints.RemoveAt(lastIndex);

        int lastInstanceIndex = measurementPointInstances.Count - 1;
        if (measurementPointInstances[lastInstanceIndex] != null)
        {
            Destroy(measurementPointInstances[lastInstanceIndex]);
        }
        measurementPointInstances.RemoveAt(lastInstanceIndex);

        UpdateVisuals();
        previousButton.interactable = measurementPoints.Count > 0;
    }

    private void OnUnitChanged(int index)
    {
        currentUnit = (MeasurementUnit)index;
        UpdateMeasurementTexts();
    }

    private void OnDisplayModeChanged(int index)
    {
        currentDisplayMode = (DisplayMode)index;
        UpdateMeasurementTexts();
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added.Count > 0 && currentTracker == null)
        {
            currentTracker = Instantiate(visualTrackerPrefab);
            currentTracker.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isInitialized) return;
        UpdateTrackerPosition();
    }

    private void UpdateTrackerPosition()
    {
        if (currentTracker == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            currentTracker.SetActive(true);
            currentTracker.transform.position = hits[0].pose.position;
            currentTracker.transform.rotation = hits[0].pose.rotation;
        }
        else
        {
            currentTracker.SetActive(false);
        }
    }

    private void OnTap()
    {
        if (!isInitialized || currentTracker == null || !currentTracker.activeSelf)
            return;

        if (IsPointerOverUI())
            return;

        PlaceMeasurementPoint(currentTracker.transform.position);
    }

    private bool IsPointerOverUI()
    {
        Vector2 touchPosition = Pointer.current.position.ReadValue();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touchPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void PlaceMeasurementPoint(Vector3 position)
    {
        if (measurementPoints.Count > 2 && Vector3.Distance(position, measurementPoints[0]) < OverlapThreshold)
        {
            measurementPoints.Add(measurementPoints[0]);
            GameObject closingPoint = Instantiate(edgePointPrefab, measurementPoints[0], Quaternion.identity);
            measurementPointInstances.Add(closingPoint);
            UpdateVisuals();
            previousButton.interactable = true;
            return;
        }

        foreach (Vector3 existingPoint in measurementPoints)
        {
            if (Vector3.Distance(position, existingPoint) < OverlapThreshold)
                return;
        }

        measurementPoints.Add(position);
        GameObject newPoint = Instantiate(edgePointPrefab, position, Quaternion.identity);
        measurementPointInstances.Add(newPoint);
        UpdateVisuals();
        previousButton.interactable = true;
    }

    private void UpdateVisuals()
    {
        UpdateLineRenderer();
        UpdateMeasurementTexts();
    }

    private void UpdateLineRenderer()
    {
        if (measurementPoints.Count > 2 && Vector3.Distance(measurementPoints[0], measurementPoints[^1]) < OverlapThreshold)
        {
            lineRenderer.positionCount = measurementPoints.Count + 1;
            lineRenderer.SetPositions(measurementPoints.ToArray());
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, measurementPoints[0]);
        }
        else
        {
            lineRenderer.positionCount = measurementPoints.Count;
            lineRenderer.SetPositions(measurementPoints.ToArray());
        }
    }

    private void UpdateMeasurementTexts()
    {
        ClearMeasurementTexts();

        for (int i = 1; i < measurementPoints.Count; i++)
        {
            CreateSegmentText(measurementPoints[i - 1], measurementPoints[i]);
        }

        if (measurementPoints.Count > 2 && Vector3.Distance(measurementPoints[0], measurementPoints[^1]) < OverlapThreshold)
        {
            if (currentDisplayMode == DisplayMode.Area)
            {
                DisplayArea();
            }
            else
            {
                DisplayPerimeter();
            }
        }
    }

    private void CreateSegmentText(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        distance = ConvertToSelectedUnit(distance);

        Vector3 midpoint = (start + end) / 2f;

        TMP_Text text = Instantiate(measurementTextPrefab, midpoint, Quaternion.identity, worldSpaceCanvas.transform);
        text.gameObject.SetActive(true);
        text.text = $"{distance:F2} {GetUnitSuffix()}";
        text.rectTransform.sizeDelta = new Vector2(200, 50);
        text.alignment = TextAlignmentOptions.Center;
        text.transform.localScale = Vector3.one * 0.01f;
        text.transform.LookAt(Camera.main.transform);
        text.transform.rotation *= Quaternion.Euler(0, 180, 0);

        measurementTextInstances.Add(text);
    }

    private void DisplayArea()
    {
        float area = CalculatePolygonArea(measurementPoints);
        area = ConvertAreaToSelectedUnit(area);

        Vector3 center = CalculateCenter(measurementPoints);

        TMP_Text areaText = Instantiate(measurementTextPrefab, center, Quaternion.identity, worldSpaceCanvas.transform);
        areaText.gameObject.SetActive(true);
        areaText.text = $"Area: {area:F2} {GetAreaUnitSuffix()}";
        areaText.fontSize *= 1.2f;
        areaText.rectTransform.sizeDelta = new Vector2(300, 80);
        areaText.transform.localScale = Vector3.one * 0.015f;
        areaText.transform.LookAt(Camera.main.transform);
        areaText.transform.rotation *= Quaternion.Euler(0, 180, 0);

        measurementTextInstances.Add(areaText);
    }

    private void DisplayPerimeter()
    {
        float perimeter = CalculatePerimeter(measurementPoints);
        perimeter = ConvertToSelectedUnit(perimeter);

        Vector3 center = CalculateCenter(measurementPoints);

        TMP_Text perimeterText = Instantiate(measurementTextPrefab, center, Quaternion.identity, worldSpaceCanvas.transform);
        perimeterText.gameObject.SetActive(true);
        perimeterText.text = $"Perimeter: {perimeter:F2} {GetUnitSuffix()}";
        perimeterText.fontSize *= 1.2f;
        perimeterText.rectTransform.sizeDelta = new Vector2(300, 80);
        perimeterText.transform.localScale = Vector3.one * 0.015f;
        perimeterText.transform.LookAt(Camera.main.transform);
        perimeterText.transform.rotation *= Quaternion.Euler(0, 180, 0);

        measurementTextInstances.Add(perimeterText);
    }

    private float CalculatePolygonArea(List<Vector3> vertices)
    {
        int n = vertices.Count;
        float area = 0.0f;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += vertices[i].x * vertices[j].z;
            area -= vertices[j].x * vertices[i].z;
        }

        return Mathf.Abs(area) / 2.0f;
    }

    private float CalculatePerimeter(List<Vector3> vertices)
    {
        float perimeter = 0f;
        for (int i = 1; i < vertices.Count; i++)
        {
            perimeter += Vector3.Distance(vertices[i - 1], vertices[i]);
        }
        return perimeter;
    }

    private float ConvertToSelectedUnit(float meters)
    {
        switch (currentUnit)
        {
            case MeasurementUnit.Centimeters: return meters * 100f;
            case MeasurementUnit.Meters: return meters;
            case MeasurementUnit.Inches: return meters * 39.3701f;
            case MeasurementUnit.Feet: return meters * 3.28084f;
            default: return meters * 100f;
        }
    }

    private float ConvertAreaToSelectedUnit(float squareMeters)
    {
        switch (currentUnit)
        {
            case MeasurementUnit.Centimeters: return squareMeters * 10000f;
            case MeasurementUnit.Meters: return squareMeters;
            case MeasurementUnit.Inches: return squareMeters * 1550f;
            case MeasurementUnit.Feet: return squareMeters * 10.7639f;
            default: return squareMeters * 10000f;
        }
    }

    private string GetUnitSuffix()
    {
        switch (currentUnit)
        {
            case MeasurementUnit.Centimeters: return "cm";
            case MeasurementUnit.Meters: return "m";
            case MeasurementUnit.Inches: return "in";
            case MeasurementUnit.Feet: return "ft";
            default: return "cm";
        }
    }

    private string GetAreaUnitSuffix()
    {
        switch (currentUnit)
        {
            case MeasurementUnit.Centimeters: return "cm²";
            case MeasurementUnit.Meters: return "m²";
            case MeasurementUnit.Inches: return "in²";
            case MeasurementUnit.Feet: return "ft²";
            default: return "cm²";
        }
    }

    private Vector3 CalculateCenter(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
            center += point;
        return center / points.Count;
    }

    private void ClearMeasurementTexts()
    {
        foreach (var text in measurementTextInstances)
        {
            if (text != null && text.gameObject != null)
                Destroy(text.gameObject);
        }
        measurementTextInstances.Clear();
    }

    public void ResetMeasurement()
    {
        measurementPoints.Clear();
        lineRenderer.positionCount = 0;

        foreach (GameObject point in measurementPointInstances)
            Destroy(point);
        measurementPointInstances.Clear();

        ClearMeasurementTexts();
        previousButton.interactable = false;
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            tapAction.Disable();
            tapAction.performed -= ctx => OnTap();
            arPlaneManager.planesChanged -= OnPlanesChanged;
        }
    }
}