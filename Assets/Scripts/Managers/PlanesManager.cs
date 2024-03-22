using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;

/// <summary>
/// Class for managing ARPlanes and querying the ML device for new plane information
/// </summary>
public class PlanesManager : Singleton<PlanesManager>
{
    
    private ARPlaneManager planeManager;

    [SerializeField, Tooltip("Maximum number of planes to return each query")]
    private uint maxResults = 100;

    [SerializeField, Tooltip("Minimum plane area to treat as a valid plane")]
    private float minPlaneArea = 0.25f;

    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    public override void Awake()
    {
        base.Awake();
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
    }

    private void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("Failed to find ARPlaneManager in scene. Disabling Script");
            enabled = false;
        }
        else
        {
            // disable planeManager until we have successfully requested required permissions
            planeManager.enabled = false;
        }

        MLPermissions.RequestPermission(MLPermission.SpatialMapping, permissionCallbacks);
    }

    private bool initialQueryPerformed = false;
    private void Update()
    {
        //only update planes if use is manipulating an object
        //OR if an initial query was not performed yet
        if (UIManager.Instance.CurrentConfigurable != null &&
            UIManager.Instance.CurrentConfigurable.IsBeingManipulated) 
        {
            UpdateQuery();
        }
        else if (!initialQueryPerformed)
        {
            UpdateQuery();
            initialQueryPerformed = true;
        }
    }

    private void UpdateQuery()
    {
        if (planeManager.enabled)
        {
            PlanesSubsystem.Extensions.Query = new PlanesSubsystem.Extensions.PlanesQuery
            {
                Flags = PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_Floor |
                        PlanesSubsystem.Extensions.MLPlanesQueryFlags.Horizontal |
                        PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_Wall |
                        PlanesSubsystem.Extensions.MLPlanesQueryFlags.Vertical |
                        PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_Ceiling,
                BoundsCenter = Camera.main.transform.position,
                BoundsRotation = Camera.main.transform.rotation,
                BoundsExtents = Vector3.one * 20f,
                MaxResults = maxResults,
                MinPlaneArea = minPlaneArea
            };
        }
        
        //update plane toggles when we query since more planes could have been generated
        UpdateActiveFloorPlanes();
        UpdateActiveWallPlanes();
    }

    private void OnPermissionGranted(string permission)
    {
        planeManager.enabled = true;
        UpdateQuery();
    }

    private void OnPermissionDenied(string permission)
    {
        Debug.LogError(
            $"Failed to create Planes Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
        enabled = false;
    }
    
    //Get a wall plane nearest a given position
    public ARPlane GetNearestPlane(Vector3 position, PlaneClassification planeClassification)
    {
        ARPlane nearestPlane = null;
  
        if (planeManager != null)
        {
            float bestDistance = float.MaxValue;
            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane.classification == planeClassification)
                {
                    float dist = Vector3.Distance(plane.transform.position, position);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        nearestPlane = plane;
                    } 
                }
            }
        }

        return nearestPlane;
    }

    //Get all current wall planes
    public List<ARPlane> GetAllWallPlanes()
    {
        List<ARPlane> planes = new();
        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.classification == PlaneClassification.Wall)
            {
                planes.Add(plane);
            }
        }
        return planes;
    }
    
    //Toggle all planes on or off
    void ToggleAllPlanes(bool shouldShow)
    {
        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(shouldShow);
        }
    }

    private bool WallPlanesActive => Settings.WallSnappingEnabled;
    public void UpdateActiveWallPlanes()
    {
        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.classification == PlaneClassification.Wall || plane.alignment == PlaneAlignment.Vertical)
            {
                plane.gameObject.SetActive(WallPlanesActive);
            }
        }
    }

    private bool FloorPlanesActive => Settings.FloorSnappingEnabled;
    public void UpdateActiveFloorPlanes()
    {
        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.classification == PlaneClassification.Floor || plane.alignment == PlaneAlignment.HorizontalUp ||
                plane.alignment == PlaneAlignment.HorizontalDown)
            {
                plane.gameObject.SetActive(FloorPlanesActive);
            }
        }
    }

}
