// Map Coordinate Overlay System
//
// Date: April 2025
//
// Handles GPS to Unity position mapping
// Implements inverse distance weighting interpolation
// Manages current position and target markers on map

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// Handles position mapping between GPS coordinates and Unity UI space
// Uses inverse distance weighting interpolation to plot points on the map
public class MapCoordinateOverlay : MonoBehaviour
{
    // Singleton instance for global access
    public static MapCoordinateOverlay Instance { get; private set; }

    [Header("Marker & Parent")]
    public GameObject markerPrefab;
    public RectTransform mapContent;

    [Header("JSON Grid Data")]
    public TextAsset jsonFile;

    [Header("Show Grid (Optional)")]
    public bool showGrid = false;
    public int gridStep = 10;       // skip some for visualization

    [Header("Inverse Distance Weighting Settings")]
    [Tooltip("Number of neighbors to use for IDW interpolation.")]
    public int kNeighbors = 4;
    [Tooltip("Power parameter for IDW. 1=linear, 2=square, etc.")]
    public float distancePower = 1f;

    [Header("GPS Data Source")]
    [SerializeField] private GPSSocketClient gpsClient;
    [Tooltip("Initial location until first GPS data arrives")]
    public float initialLat = 63.4f;
    public float initialLon = 10.4f;

    [Header("Target Marker")]
    public GameObject targetMarkerPrefab;

    [Header("Testing")]
    [SerializeField] private bool enableTestingControls = true;
    [SerializeField] private float testTargetLat = 63.42f;
    [SerializeField] private float testTargetLon = 10.42f;
    [SerializeField] private bool showTargetOnStart = false;

    // Map boundary coordinates in Unity space
    private float leftX = -48f;
    private float rightX = 16813f;
    private float topY = 48f;
    private float bottomY = -12276f;

    private MapData mapData;

    // Store the marker instances for real-time updates
    private RectTransform currentMarkerRT;
    private RectTransform targetMarkerRT;

    // Public properties to expose markers
    public RectTransform CurrentMarker
    {
        get { return currentMarkerRT; }
    }

    public RectTransform TargetMarker
    {
        get { return targetMarkerRT; }
    }

    private void Awake()
    {
        // Set up singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Load coordination grid from JSON
        LoadMapData();

        // Optionally visualize grid points
        if (showGrid)
        {
            PlaceAllGridMarkers();
        }

        // Find GPS client if not assigned
        if (gpsClient == null)
        {
            gpsClient = FindObjectOfType<GPSSocketClient>();
            if (gpsClient == null)
            {
                Debug.LogError("GPSSocketClient not found! No GPS updates will occur.");
            }
            else
            {
                // Subscribe to GPS updates
                gpsClient.OnGPSDataUpdated += OnGPSDataReceived;
                Debug.Log("Successfully connected to GPSSocketClient");
            }
        }
        else
        {
            // Subscribe to GPS updates
            gpsClient.OnGPSDataUpdated += OnGPSDataReceived;
        }

        // Create initial marker with default position
        CreateOrUpdateMarker(initialLat, initialLon);
        Debug.Log($"Placed initial marker at lat={initialLat}, lon={initialLon} - waiting for GPS data");

        if (showTargetOnStart && enableTestingControls)
        {
            CreateOrUpdateTargetMarker(testTargetLat, testTargetLon);
            Debug.Log($"Showing initial test target at: Lat={testTargetLat}, Lon={testTargetLon}");
        }
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        if (gpsClient != null)
        {
            gpsClient.OnGPSDataUpdated -= OnGPSDataReceived;
        }
    }

    // Processes incoming GPS data and updates markers
    private void OnGPSDataReceived(GPSData data)
    {
        if (!data.valid)
        {
            Debug.LogWarning("Received invalid GPS data");
            return;
        }

        // Update current position marker
        CreateOrUpdateMarker(data.latitude, data.longitude);
        Debug.Log($"Updated marker with GPS data: Lat={data.latitude}, Lon={data.longitude}");

        // Handle target location if present
        if (data.hasTarget)
        {
            // Show/update target marker
            CreateOrUpdateTargetMarker(data.targetLatitude, data.targetLongitude);
            Debug.Log($"Updated target marker: Lat={data.targetLatitude}, Lon={data.targetLongitude}");
        }
        else if (targetMarkerRT != null)
        {
            // Remove target marker if no target exists
            Destroy(targetMarkerRT.gameObject);
            targetMarkerRT = null;
            Debug.Log("Target marker removed");
        }
    }

    // Loads the map grid data from JSON file
    private void LoadMapData()
    {
        if (jsonFile == null)
        {
            Debug.LogError("No JSON file assigned!");
            return;
        }

        mapData = JsonUtility.FromJson<MapData>(jsonFile.text);
        if (mapData == null || mapData.grid == null)
        {
            Debug.LogError("JSON data invalid or no 'grid' array found!");
            return;
        }

        Debug.Log($"Loaded {mapData.grid.Length} grid points from JSON.");
    }

    // Creates or updates the user position marker based on GPS coordinates
    public void CreateOrUpdateMarker(float lat, float lon)
    {
        Vector2 unityPos = GetUnityPositionFromLatLon(lat, lon);

        if (currentMarkerRT != null)
        {
            // Update existing marker
            currentMarkerRT.anchoredPosition = unityPos;
        }
        else
        {
            // Create new marker
            GameObject markerObj = Instantiate(markerPrefab, mapContent);
            markerObj.SetActive(true);

            currentMarkerRT = markerObj.GetComponent<RectTransform>();
            if (currentMarkerRT != null)
            {
                currentMarkerRT.anchoredPosition = unityPos;
            }
            else
            {
                markerObj.transform.position = new Vector3(unityPos.x, unityPos.y, 0f);
            }

            // Ensure marker appears on top
            Canvas markerCanvas = markerObj.GetComponent<Canvas>();
            if (markerCanvas == null) markerCanvas = markerObj.AddComponent<Canvas>();
            markerCanvas.overrideSorting = true;
            markerCanvas.sortingOrder = 999;

            Image img = markerObj.GetComponent<Image>();
            if (img != null) img.enabled = true;
        }

        Debug.Log($"IDW => lat={lat}, lon={lon} => unity=({unityPos.x:F1},{unityPos.y:F1})");
    }

    // Creates or updates the target marker based on GPS coordinates
    public void CreateOrUpdateTargetMarker(float lat, float lon)
    {
        Vector2 unityPos = GetUnityPositionFromLatLon(lat, lon);

        if (targetMarkerRT != null)
        {
            // Update existing target marker
            targetMarkerRT.anchoredPosition = unityPos;
        }
        else
        {
            // Create new target marker
            GameObject markerToUse = targetMarkerPrefab != null ? targetMarkerPrefab : markerPrefab;

            GameObject markerObj = Instantiate(markerToUse, mapContent);
            markerObj.SetActive(true);
            markerObj.name = "TargetMarker";

            targetMarkerRT = markerObj.GetComponent<RectTransform>();
            if (targetMarkerRT != null)
            {
                targetMarkerRT.anchoredPosition = unityPos;
            }
            else
            {
                markerObj.transform.position = new Vector3(unityPos.x, unityPos.y, 0f);
            }

            // Ensure target appears on top of all other markers
            Canvas markerCanvas = markerObj.GetComponent<Canvas>();
            if (markerCanvas == null) markerCanvas = markerObj.AddComponent<Canvas>();
            markerCanvas.overrideSorting = true;
            markerCanvas.sortingOrder = 1000;

            Image img = markerObj.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = true;
                Color color = img.color;
                color.a = 1f; // Make fully opaque
                img.color = color;
            }
        }

        Debug.Log($"Target marker updated: Lat={lat}, Lon={lon} => Unity=({unityPos.x:F1},{unityPos.y:F1})");
    }

    // Removes the target marker if it exists
    public void ClearTargetMarker()
    {
        if (targetMarkerRT != null)
        {
            Destroy(targetMarkerRT.gameObject);
            targetMarkerRT = null;
            Debug.Log("Target marker cleared");
        }
    }

    // For testing target markers in Unity Editor
    void Update()
    {
        if (!enableTestingControls) return;

        // Press T key to place a test target marker
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlaceTestTargetMarker();
        }
    }

    // Places a test target marker using the configured test coordinates
    public void PlaceTestTargetMarker()
    {
        CreateOrUpdateTargetMarker(testTargetLat, testTargetLon);
        Debug.Log($"Test target marker placed at: Lat={testTargetLat}, Lon={testTargetLon}");
    }

    // Sets test target coordinates and places marker
    public void SetTestTargetCoordinates(float lat, float lon)
    {
        testTargetLat = lat;
        testTargetLon = lon;
        PlaceTestTargetMarker();
    }

    // Converts GPS coordinates to Unity position using IDW interpolation
    private Vector2 GetUnityPositionFromLatLon(float lat, float lon)
    {
        if (mapData == null || mapData.grid == null)
        {
            Debug.LogError("No grid data loaded!");
            return Vector2.zero;
        }

        // Find k nearest grid points
        List<GridPoint> neighbors = FindKClosestNeighbors(lat, lon, kNeighbors);

        // Calculate interpolated position using IDW
        Vector2 norm = InverseDistanceWeightedNormalized(lat, lon, neighbors, distancePower);

        // Convert normalized position to Unity coordinates
        return ConvertNormalizedToUnity(norm.x, norm.y);
    }

    // Finds k nearest neighbor points from the grid data
    private List<GridPoint> FindKClosestNeighbors(float lat, float lon, int k)
    {
        List<(float distSq, GridPoint gp)> distList = new List<(float, GridPoint)>(mapData.grid.Length);
        foreach (GridPoint gp in mapData.grid)
        {
            float dx = gp.lat - lat;
            float dy = gp.lon - lon;
            float distSq = dx * dx + dy * dy;
            distList.Add((distSq, gp));
        }
        distList.Sort((a, b) => a.distSq.CompareTo(b.distSq));
        
        List<GridPoint> result = new List<GridPoint>(k);
        for (int i = 0; i < k && i < distList.Count; i++)
        {
            result.Add(distList[i].gp);
        }
        return result;
    }

    // Performs inverse-distance weighting interpolation
    private Vector2 InverseDistanceWeightedNormalized(float lat, float lon, List<GridPoint> neighbors, float power)
    {
        float sumWeights = 0f, sumX = 0f, sumY = 0f;
        
        foreach (GridPoint gp in neighbors)
        {
            float dx = gp.lat - lat;
            float dy = gp.lon - lon;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            
            // Direct match - use exact point
            if (dist < 1e-9f)
            {
                return new Vector2(gp.normalizedX, gp.normalizedY);
            }
            
            // Calculate inverse distance weight
            float w = 1f / Mathf.Pow(dist, power);
            sumWeights += w;
            sumX += w * gp.normalizedX;
            sumY += w * gp.normalizedY;
        }
        
        if (sumWeights < 1e-12f)
        {
            return new Vector2(neighbors[0].normalizedX, neighbors[0].normalizedY);
        }
        
        // Return weighted average
        return new Vector2(sumX / sumWeights, sumY / sumWeights);
    }

    // Converts normalized [0..1] coordinates to Unity UI coordinates
    private Vector2 ConvertNormalizedToUnity(float nx, float ny)
    {
        float x = Mathf.Lerp(leftX, rightX, nx);
        float y = Mathf.Lerp(topY, bottomY, ny);
        return new Vector2(x, y);
    }

    // Places markers for all grid points for visualization
    private void PlaceAllGridMarkers()
    {
        if (mapData == null || mapData.grid == null) return;
        
        Debug.Log($"Placing grid markers for {mapData.grid.Length} points (skip={gridStep})...");
        for (int i = 0; i < mapData.grid.Length; i += gridStep)
        {
            GridPoint gp = mapData.grid[i];
            Vector2 unityPos = ConvertNormalizedToUnity(gp.normalizedX, gp.normalizedY);
            InstantiateMarker(unityPos);
        }
    }

    // Creates a marker at the specified Unity position
    private void InstantiateMarker(Vector2 position)
    {
        if (markerPrefab == null || mapContent == null)
        {
            Debug.LogError("MarkerPrefab or mapContent not set!");
            return;
        }
        
        GameObject markerObj = Instantiate(markerPrefab, mapContent);
        markerObj.SetActive(true);
        RectTransform rt = markerObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = position;
        }
        else
        {
            markerObj.transform.position = new Vector3(position.x, position.y, 0f);
        }
        
        Canvas markerCanvas = markerObj.GetComponent<Canvas>();
        if (markerCanvas == null) markerCanvas = markerObj.AddComponent<Canvas>();
        markerCanvas.overrideSorting = true;
        markerCanvas.sortingOrder = 999;
        
        Image img = markerObj.GetComponent<Image>();
        if (img != null) img.enabled = true;
    }

    // JSON data classes for serialization
    [Serializable]
    public class MapData
    {
        public GridPoint[] grid;
    }

    [Serializable]
    public class GridPoint
    {
        public float normalizedX;
        public float normalizedY;
        public float lat;
        public float lon;
    }
}
