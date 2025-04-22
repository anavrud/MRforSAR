// Map Visibility Toggle for HoloLens
//
// Date: April 2025
//
// Controls the visibility state of the map interface
// Handles showing/hiding with cooldown protection
// Manages state transitions and related functionality

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

// Controls the visibility state of the map interface
// Includes cooldown mechanism to prevent rapid toggling
public class MapToggle : MonoBehaviour
{
    [Header("References")]
    public Canvas mapCanvas; // Public for easier assignment in the Inspector
    public Button toggleButton; // Reference to the button triggering the toggle

    [Header("Settings")]
    [SerializeField] private float cooldownDuration = 0.5f;
    [SerializeField] private bool visibleOnStart = true;

    private bool isMapVisible;
    private bool isCoolingDown = false;
    private InteractiveMapAssembler mapAssembler;
    private bool isProcessingClick = false; // Flag to track if we're handling a click

    void Start()
    {
        // Find map assembler if needed for followMarker functionality
        mapAssembler = FindObjectOfType<InteractiveMapAssembler>();

        // Set initial state
        isMapVisible = visibleOnStart;

        // Apply initial state with delay to ensure everything is loaded
        Invoke("ApplyInitialState", 0.2f);
        
        // Find and configure button if not assigned
        if (toggleButton == null)
        {
            toggleButton = GetComponentInChildren<Button>();
        }
        
        // If we have a button, modify its trigger behavior
        if (toggleButton != null)
        {
            // Remove existing click listeners to avoid duplicates
            toggleButton.onClick.RemoveAllListeners();
            
            // Add our protected toggle function
            toggleButton.onClick.AddListener(SafeToggleMap);
            
            // Change trigger behavior to prevent issue on drag
            EventTrigger eventTrigger = toggleButton.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = toggleButton.gameObject.AddComponent<EventTrigger>();
            }
            
            // Clear existing triggers to avoid conflicts
            if (eventTrigger.triggers != null)
            {
                eventTrigger.triggers.Clear();
            }
        }
    }

    private void ApplyInitialState()
    {
        if (mapCanvas != null)
        {
            mapCanvas.gameObject.SetActive(isMapVisible);
        }
        else
        {
            Debug.LogError("Map Canvas is not assigned!");
        }
    }

    // Safe toggle method to ensure exactly one toggle per click
    public void SafeToggleMap()
    {
        // Skip if already processing a click
        if (isProcessingClick)
            return;
            
        // Set processing flag
        isProcessingClick = true;
        
        // Perform the toggle
        ToggleMap();
        
        // Reset flag after a short delay 
        // This prevents multiple calls if the button event fires multiple times
        StartCoroutine(ResetClickProcessingFlag());
    }
    
    private IEnumerator ResetClickProcessingFlag()
    {
        // Small delay to ensure all potential button events from this click are done
        yield return new WaitForSeconds(0.2f);
        isProcessingClick = false;
    }

    // Toggles the map visibility with cooldown protection
    public void ToggleMap()
    {
        // Check if we're in cooldown period
        if (isCoolingDown)
        {
            Debug.Log("Button on cooldown - ignoring toggle request");
            return;
        }

        // Toggle visibility
        isMapVisible = !isMapVisible;

        // Apply visibility change
        if (mapCanvas != null)
        {
            // When hiding, disable followMarker to prevent auto-reactivation
            if (!isMapVisible && mapAssembler != null)
            {
                mapAssembler.followMarker = false;
            }

            mapCanvas.gameObject.SetActive(isMapVisible);
            Debug.Log("Map visibility toggled to: " + isMapVisible);

            // Start cooldown
            StartCoroutine(CooldownRoutine());
        }
        else
        {
            Debug.LogError("Map Canvas is not assigned!");
        }
    }

    // Coroutine that handles the cooldown timer
    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        // Wait for cooldown duration
        yield return new WaitForSeconds(cooldownDuration);

        isCoolingDown = false;
    }
}