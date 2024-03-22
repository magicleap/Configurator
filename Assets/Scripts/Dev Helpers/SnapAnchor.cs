using UnityEngine;

/// <summary>
/// Class to help find the correct position of an asset that is supposed to snap to a surface
/// These should be placed on all 4 sides of an asset (much like a box collider)
/// </summary>
public class SnapAnchor : MonoBehaviour
{
    public Vector3 Position => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
