// Map Reset Controller for HoloLens
//
// Author: Aleksander Navrud 
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
    [SerializeField] private float initialDistance = 1.1f;

    private bool isCoolingDown = false;

    // Captured at Start()
    private Vector3 originalContentScale;
    private Vector3 originalCanvasScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Initialize references and store the map's original scale, position, and rotation
    void Start()
    {
        if (mapAssembler == null)
            mapAssembler = FindObjectOfType<InteractiveMapAssembler>();

        if (mapCanvas == null && mapAssembler?.mapContent != null)
            mapCanvas = mapAssembler.mapContent.GetComponentInParent<Canvas>();

        if (mapViewController == null)
            mapViewController = FindObjectOfType<MapViewController>();

        if (mapAssembler?.mapContent != null)
            originalContentScale = mapAssembler.mapContent.localScale;

        if (mapCanvas != null)
        {
            originalCanvasScale = mapCanvas.transform.localScale;
            originalPosition = mapCanvas.transform.localPosition;
            originalRotation = mapCanvas.transform.localRotation;
        }
    }

    // Reset map to its initial transform, scale, and solver distance, then recenter
    public void ResetMap()
    {
        if (isCoolingDown)
        {
            Debug.Log("Reset button on cooldown - ignoring reset request");
            return;
        }

        if (mapCanvas != null)
            mapCanvas.gameObject.SetActive(true);

        // Reset solver distance
        if (mapViewController != null)
            mapViewController.AdjustDistance(initialDistance);

        if (mapAssembler != null)
        {
            // Restore UI‐content zoom
            if (mapAssembler.mapContent != null)
                mapAssembler.mapContent.localScale = originalContentScale;

            // Restore world‐space Canvas scale, pos & rot
            if (mapCanvas != null)
            {
                var t = mapCanvas.transform;
                t.localScale = originalCanvasScale;
                t.localPosition = originalPosition;
                t.localRotation = originalRotation;
            }

            // Re‐enable follow & recenter
            mapAssembler.followMarker = true;
            mapAssembler.RecenterMapButton();

            Debug.Log("Map has been reset to initial state");
        }
        else
        {
            Debug.LogError("Map Assembler not found - cannot reset map!");
        }

        StartCoroutine(CooldownRoutine());
    }

    // Prevent rapid consecutive resets by enforcing a short cooldown
    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown = false;
    }
}