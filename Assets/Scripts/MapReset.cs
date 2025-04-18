﻿// Map Reset Controller for HoloLens
//
// Date: April 2025
//
// Manages map reset functionality
// Returns map to default scale, position and configuration
// Includes cooldown protection to prevent rapid triggering

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapReset : MonoBehaviour
{
    [Header("References")]
    public Canvas mapCanvas;
    public InteractiveMapAssembler mapAssembler;
    public MapViewController mapViewController;

    [Header("Settings")]
    [SerializeField] private float cooldownDuration = 0.5f;
    [SerializeField] private Vector3 initialScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float initialDistance = 1.1f;

    private bool isCoolingDown = false;
    private Vector3 initialWorldScale;

    void Start()
    {
        // Find components if not assigned
        if (mapAssembler == null)
            mapAssembler = FindObjectOfType<InteractiveMapAssembler>();

        if (mapCanvas == null && mapAssembler != null && mapAssembler.mapContent != null)
            mapCanvas = mapAssembler.mapContent.GetComponentInParent<Canvas>();

        if (mapViewController == null)
            mapViewController = FindObjectOfType<MapViewController>();

        // Capture the initial world scale of the map canvas
        if (mapCanvas != null)
        {
            initialWorldScale = mapCanvas.transform.localScale;
        }
    }

    /// Resets the map to its initial state
    public void ResetMap()
    {
        // Check if we're in cooldown period
        if (isCoolingDown)
        {
            Debug.Log("Reset button on cooldown - ignoring reset request");
            return;
        }

        // Make sure the map is visible first
        if (mapCanvas != null)
        {
            mapCanvas.gameObject.SetActive(true);
            // Reset world scale to initial
            mapCanvas.transform.localScale = initialWorldScale;
        }

        // Reset map position and scale
        if (mapAssembler != null)
        {
            // Reset scale to initial value
            if (mapAssembler.mapContent != null)
            {
                mapAssembler.mapContent.localScale = initialScale;
            }

            // Enable follow marker to ensure map moves with user
            mapAssembler.followMarker = true;

            // Force map to recenter on marker
            mapAssembler.RecenterMapButton();

            Debug.Log("Map has been reset to initial state");
        }
        else
        {
            Debug.LogError("Map Assembler not found - cannot reset map!");
        }

        // Reset map view distance using the public method
        if (mapViewController != null)
        {
            mapViewController.AdjustDistance(initialDistance);
        }

        // Start cooldown
        StartCoroutine(CooldownRoutine());
    }

    /// Coroutine that handles the cooldown timer
    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown = false;
    }
}