using UnityEngine;

public class PlayerPortalTeleport : MonoBehaviour
{
    public Transform playerCamera; // Players camera (for rotation)
    public float checkRadius = 0.5f; // Half-size to approximate "center"

    private Portal currentPortal;
    private bool isCrossing = false;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Find all portals in scene
        Portal[] portals = FindObjectsOfType<Portal>();

        foreach (Portal portal in portals)
        {
            if (portal.linkedPortal == null) continue;
            CheckPortalCrossing(portal);
        }

        lastPosition = transform.position;
    }

    void CheckPortalCrossing(Portal portal)
    {
        Transform plane = portal.portalPlane;
        if (plane == null) return;

        Vector3 portalNormal = plane.forward;
        Vector3 playerPos = transform.position;

        // Compute which side of the plane the player is on (dot product sign)
        float sideNow = Vector3.Dot(portalNormal, playerPos - plane.position);
        float sideBefore = Vector3.Dot(portalNormal, lastPosition - plane.position);

        // If sign changed > crossed the plane
        if (sideNow < 0 && sideBefore > 0 && !isCrossing)
        {
            // Teleport
            Teleport(portal);
            isCrossing = true;
            Invoke(nameof(ResetCrossing), 0.2f); // debounce
        }
    }

    void ResetCrossing()
    {
        isCrossing = false;
    }

    void Teleport(Portal inPortal)
    {
        Portal outPortal = inPortal.linkedPortal;
        if (outPortal == null) return;

        Transform inPlane = inPortal.portalPlane;
        Transform outPlane = outPortal.portalPlane;

        // Compute offset relative to the entry portal
        Vector3 localOffset = inPlane.InverseTransformPoint(transform.position);
        Vector3 newWorldPos = outPlane.TransformPoint(localOffset);

        transform.position = newWorldPos;

        // Compute new rotation based on how the portals face
        Quaternion rotationDiff = outPlane.rotation * Quaternion.Inverse(inPlane.rotation);
        transform.rotation = rotationDiff * transform.rotation;

        // Rotate the camera (if separate)
        if (playerCamera != null)
        {
            playerCamera.rotation = rotationDiff * playerCamera.rotation;
        }

        Debug.Log($"Teleported from {inPortal.name} to {outPortal.name}");
    }
}
