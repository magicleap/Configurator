using UnityEngine;

/// <summary>
/// Class that allows us to easily visualize a raycast
/// Not used in final product, but is used as a dev tool
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class DebugRayHelper : MonoBehaviour
{
    public static DebugRayHelper instance;
    private LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    public void UpdateLinePositions(Vector3[] positions)
    {
        lineRenderer.SetPositions(positions);
    }
    
    
}
