using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

// Represents GPS data structure with both current position and target location
[Serializable]
public class GPSData
{
    // Current position fields
    public float latitude;
    public float longitude;
    public float altitude;
    public long timestamp;
    public bool valid;

    // Target location fields
    public float targetLatitude;
    public float targetLongitude;
    public float targetAltitude;
    public bool hasTarget;
}

// Client for connecting to GPS server and receiving location updates
// Handles socket communication and data parsing for HoloLens
public class GPSSocketClient : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private string serverIP = "10.24.9.128";       // Android device IP address
    [SerializeField] private string serverPort = "8085";
    [SerializeField] private float connectionRetryInterval = 5.0f;

    // Event for notifying subscribers about GPS updates
    public event Action<GPSData> OnGPSDataUpdated;

    // Public properties for accessing GPS state
    public GPSData CurrentGPSData { get; private set; }
    public bool IsConnected { get; private set; }
    public DateTime LastUpdateTime { get; private set; }
    public string StatusMessage { get; private set; }

#if !UNITY_EDITOR && UNITY_WSA
    // HoloLens-specific socket implementation
    private Windows.Networking.Sockets.StreamSocket socket;
    private bool isConnecting = false;
#endif

    private void Start()
    {
        // Initialize data structure
        CurrentGPSData = new GPSData();
        IsConnected = false;
        StatusMessage = "Initializing...";

#if !UNITY_EDITOR && UNITY_WSA
        // Start socket connection on HoloLens platform
        ConnectToServer();
#else
        Debug.LogWarning("GPS Socket Client only works on HoloLens, not in Unity Editor");
#endif
    }

    private void OnDestroy()
    {
#if !UNITY_EDITOR && UNITY_WSA
        // Clean up socket connection
        DisconnectFromServer();
#endif
    }

#if !UNITY_EDITOR && UNITY_WSA
    // Establishes socket connection to GPS server
    private async void ConnectToServer()
    {
        if (isConnecting) return;
        
        isConnecting = true;
        StatusMessage = "Connecting...";
        Debug.Log($"Attempting to connect to GPS server at {serverIP}:{serverPort}");

        try
        {
            // Create and configure socket
            socket = new Windows.Networking.Sockets.StreamSocket();
            
            // Set timeout to avoid hanging
            socket.Control.KeepAlive = false;
            
            // Connect to server using the configured IP and port
            var hostName = new Windows.Networking.HostName(serverIP);
            await socket.ConnectAsync(hostName, serverPort);
            
            IsConnected = true;
            StatusMessage = "Connected";
            Debug.Log("Connected to GPS server");
            
            // Start receiving data
            ReceiveData();
        }
        catch (Exception e)
        {
            IsConnected = false;
            StatusMessage = $"Connection failed: {e.Message}";
            Debug.LogError($"Failed to connect to GPS server: {e.Message}");
            
            // Schedule reconnection attempt
            StartCoroutine(RetryConnection());
        }
        finally
        {
            isConnecting = false;
        }
    }

    // Retry connection after delay
    private IEnumerator RetryConnection()
    {
        yield return new WaitForSeconds(connectionRetryInterval);
        ConnectToServer();
    }

    // Continuously receives and processes data from the server
    private async void ReceiveData()
    {
        try
        {
            using (var reader = new Windows.Storage.Streams.DataReader(socket.InputStream))
            {
                reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
                
                while (IsConnected)
                {
                    // Read data from server with buffer
                    uint bytesRead = await reader.LoadAsync(8192);
                    
                    if (bytesRead > 0)
                    {
                        // Parse received JSON data
                        string jsonData = reader.ReadString(bytesRead);
                        Debug.Log($"Received GPS data: {jsonData}");
                        
                        try
                        {
                            // Convert JSON to GPSData object
                            GPSData newData = JsonUtility.FromJson<GPSData>(jsonData);
                            
                            // Update stored data
                            CurrentGPSData = newData;
                            LastUpdateTime = DateTime.Now;
                            
                            // Notify subscribers
                            OnGPSDataUpdated?.Invoke(CurrentGPSData);
                            
                            Debug.Log($"GPS Updated: Lat={CurrentGPSData.latitude}, Lon={CurrentGPSData.longitude}, Alt={CurrentGPSData.altitude}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed to parse GPS data: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Connection closed by server
                        Debug.Log("Connection closed by server");
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving GPS data: {e.Message}");
        }
        
        // Connection lost, update state
        IsConnected = false;
        StatusMessage = "Disconnected";
        
        // Clean up and reconnect
        DisconnectFromServer();
        StartCoroutine(RetryConnection());
    }

    // Closes and disposes the socket connection
    private void DisconnectFromServer()
    {
        if (socket != null)
        {
            socket.Dispose();
            socket = null;
        }
        
        IsConnected = false;
    }
#endif
}