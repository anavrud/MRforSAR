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

// Controls the visibility state of the map interface
// Includes cooldown mechanism to prevent rapid toggling
public class MapToggle : MonoBehaviour
{
    [Header("References")]
    public Canvas mapCanvas; // Public for easier assignment in the Inspector

    [Header("Settings")]
    [SerializeField] private float cooldownDuration = 0.5f;
    [SerializeField] private bool visibleOnStart = true;

    private bool isMapVisible;
    private bool isCoolingDown = false;
    private InteractiveMapAssembler mapAssembler;

    void Start()
    {
        // Find map assembler if needed for followMarker functionality
        mapAssembler = FindObjectOfType<InteractiveMapAssembler>();

        // Set initial state
        isMapVisible = visibleOnStart;

        // Apply initial state with delay to ensure everything is loaded
        Invoke("ApplyInitialState", 0.2f);
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