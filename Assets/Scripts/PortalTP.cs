using UnityEngine;

public class Portal : MonoBehaviour
{
    [Tooltip("The linked portal to teleport to.")]
    public Portal linkedPortal;

    [Tooltip("Optional: assign the plane's transform used as the teleport plane.")]
    public Transform portalPlane;

    private void Reset()
    {
        portalPlane = transform; // Default to own transform
    }

    // For debug visualization
    private void OnDrawGizmos()
    {
        if (portalPlane)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(portalPlane.position, portalPlane.forward * 2);
        }
    }
}
