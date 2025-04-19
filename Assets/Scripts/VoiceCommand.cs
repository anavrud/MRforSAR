// Voice Command Handler for Map Interactions
//
// Author: Endre Kalheim
// Date: April 2025
//
// Processes voice commands for map control
// Implements MRTK speech recognition interface
// Supports map toggle, recenter and reset commands

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

// Handles voice command recognition and processing for map interactions
// Implements the MRTK speech handler interface
public class VoiceCommand : MonoBehaviour, IMixedRealitySpeechHandler
{
    [Header("Component References")]
    [SerializeField]
    private InteractiveMapAssembler interactiveMapAssembler;

    [SerializeField]
    private MapToggle mapToggle;

    [SerializeField]
    private MapReset mapReset;

    private void Start()
    {
        // Find references if not assigned
        if (interactiveMapAssembler == null)
            interactiveMapAssembler = FindObjectOfType<InteractiveMapAssembler>();

        if (mapToggle == null)
            mapToggle = FindObjectOfType<MapToggle>();

        if (mapReset == null)
            mapReset = FindObjectOfType<MapReset>();
    }

    // Called by MRTK when a speech command is recognized
    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        switch (eventData.Command.Keyword.ToLower())
        {
            case "center":
                Debug.Log("Center command recognized");
                if (interactiveMapAssembler != null)
                    interactiveMapAssembler.RecenterMapButton();
                else
                    Debug.LogWarning("Map assembler not found for center command");
                eventData.Use();
                break;

            case "map":
                Debug.Log("Map command recognized");
                if (mapToggle != null)
                    mapToggle.ToggleMap();
                else
                    Debug.LogWarning("Map toggle not found for Map command");
                eventData.Use();
                break;

            case "reset":
                Debug.Log("Reset command recognized");
                if (mapReset != null)
                    mapReset.ResetMap();
                else
                    Debug.LogWarning("Map reset not found for reset command");
                eventData.Use();
                break;
        }
    }

    // Simulates the recenter voice command via code
    public void SimulateRecenterCommand()
    {
        Debug.Log("Simulated recenter command");
        if (interactiveMapAssembler != null)
            interactiveMapAssembler.RecenterMapButton();
    }

    // Simulates the toggle map voice command via code
    public void SimulateToggleCommand()
    {
        Debug.Log("Simulated Map command");
        if (mapToggle != null)
            mapToggle.ToggleMap();
    }

    // Simulates the reset map voice command via code
    public void SimulateResetCommand()
    {
        Debug.Log("Simulated reset command");
        if (mapReset != null)
            mapReset.ResetMap();
    }

    private void OnEnable()
    {
        // Register this object to receive speech events from MRTK
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }

    private void OnDisable()
    {
        // Unregister from speech events when disabled
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySpeechHandler>(this);
    }
}