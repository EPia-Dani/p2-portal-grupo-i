using System.Collections;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [Header("Portal Links")]
    [Tooltip("Transform of the portal's surface (this portal). Usually the same GameObject's transform.")]
    public Transform portalTransform;

    [Tooltip("Transform of the other portal (destination).")]
    public Transform linkedPortal;

    [Tooltip("The non-trigger collider representing the portal's wall � only this collider will be ignored while crossing.")]
    public Collider portalWallCollider;

    [Tooltip("Tag for identifying the player GameObject")]
    public string playerTag = "Player";

    // internal
    private Collider triggerCollider;
    private Transform trackedPlayer;        // player transform currently inside trigger
    private Vector3 lastPlayerRelative;     // last frame relative position used to detect cross
    private bool playerInside = false;
    private bool justTeleported = false;    // prevents immediate re-teleport back
    private Collider playerCollider;
    private Rigidbody playerRb;
    private CharacterController playerCC;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
            Debug.LogWarning($"{name}: PortalTeleporter expects a Collider with IsTrigger = true on the same GameObject (the portal trigger).");

        if (portalTransform == null)
            portalTransform = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !playerInside)
        {
            // capture player
            trackedPlayer = other.transform;
            playerInside = true;

            // find player's main collider/rigidbody/CC
            playerCollider = other; // assume the collider on the root that enters is the player's collider
            playerRb = other.attachedRigidbody;
            playerCC = other.GetComponent<CharacterController>();
            // If the collider you get is a child collider, try to get root's comps:
            if (playerRb == null)
                playerRb = other.GetComponentInParent<Rigidbody>();
            if (playerCC == null)
                playerCC = other.GetComponentInParent<CharacterController>();
            if (playerCollider == null)
                playerCollider = other.GetComponentInParent<Collider>();

            // store last relative sign to detect crossing
            lastPlayerRelative = portalTransform.InverseTransformPoint(trackedPlayer.position);

            // disable collision between the player's collider and the portal wall
            if (playerCollider != null && portalWallCollider != null)
                Physics.IgnoreCollision(playerCollider, portalWallCollider, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && playerInside)
        {
            // Re-enable the collision we disabled (if any)
            if (playerCollider != null && portalWallCollider != null)
                Physics.IgnoreCollision(playerCollider, portalWallCollider, false);

            ResetState();
        }
    }

    void ResetState()
    {
        trackedPlayer = null;
        playerInside = false;
        justTeleported = false;
        playerCollider = null;
        playerRb = null;
        playerCC = null;
    }

    void Update()
    {
        // Only process while player is inside the trigger
        if (!playerInside || trackedPlayer == null || linkedPortal == null || portalTransform == null)
            return;

        // compute signed distance (local Z) each frame
        Vector3 localPos = portalTransform.InverseTransformPoint(trackedPlayer.position);
        float prevZ = lastPlayerRelative.z;
        float currentZ = localPos.z;

        // detect center crossing: signs differ (previous positive, now negative OR vice versa)
        bool crossed = (prevZ > 0f && currentZ <= 0f) || (prevZ <= 0f && currentZ > 0f);

        // update last
        lastPlayerRelative = localPos;

        if (crossed && !justTeleported)
        {
            DoTeleport();
        }
    }

    void DoTeleport()
    {
        if (trackedPlayer == null) return;

        // Prepare references (re-check in case)
        GameObject playerGO = trackedPlayer.gameObject;
        Collider playerCol = playerCollider != null ? playerCollider : trackedPlayer.GetComponent<Collider>();
        Rigidbody rb = playerRb;
        CharacterController cc = playerCC;

        // --- compute new position and rotation (mirror like view math) ---
        // 1) player position relative to this portal
        Vector3 localPos = portalTransform.InverseTransformPoint(trackedPlayer.position);
        // mirror through portal plane (flip X and Z)
        Vector3 mirroredLocalPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        // Transform to linked portal world
        Vector3 destinationPos = linkedPortal.TransformPoint(mirroredLocalPos);

        // 2) rotation: compute relative and apply flip
        Quaternion localRot = Quaternion.Inverse(portalTransform.rotation) * trackedPlayer.rotation;
        Quaternion mirroredLocalRot = Quaternion.Euler(0f, 180f, 0f) * localRot;
        Quaternion destinationRot = linkedPortal.rotation * mirroredLocalRot;

        // --- temporarily disable collisions / controller to avoid physics glitches ---
        if (cc != null)
        {
            // CharacterController: disable during teleport to avoid internal collision issues
            cc.enabled = false;
            playerGO.transform.position = destinationPos;
            playerGO.transform.rotation = destinationRot;
            // re-enable after a frame to avoid stuck
            StartCoroutine(ReenableCharacterControllerNextFrame(cc));
        }
        else if (rb != null)
        {
            // Rigidbody: set position / rotation using MovePosition/MoveRotation or directly
            // Prefer direct set to avoid interpolation surprises:
            rb.position = destinationPos;
            rb.rotation = destinationRot;

            // rotate velocity to the new orientation
            Vector3 oldVelocity = rb.linearVelocity;
            // compute rotation delta between source and destination orientations
            Quaternion fromTo = destinationRot * Quaternion.Inverse(trackedPlayer.rotation);
            Vector3 newVelocity = fromTo * oldVelocity;
            rb.linearVelocity = newVelocity;
        }
        else
        {
            // Pure transform (no physics)
            playerGO.transform.position = destinationPos;
            playerGO.transform.rotation = destinationRot;
        }

        // After teleport, mark so we don't immediately re-teleport back
        justTeleported = true;

        // ensure we re-enable collisions with the portal wall only AFTER teleport
        if (playerCol != null && portalWallCollider != null)
        {
            // We'll re-enable when the player exits the trigger (OnTriggerExit) which occurs after teleport.
            // But in case player remains overlapping same trigger for a frame, re-enable a short time later as a backup:
            StartCoroutine(EnsureReenableCollisionLater(playerCol, portalWallCollider, 0));
        }
    }

    System.Collections.IEnumerator ReenableCharacterControllerNextFrame(CharacterController cc)
    {
        // wait one frame then re-enable
        yield return null;
        cc.enabled = true;
    }

    System.Collections.IEnumerator EnsureReenableCollisionLater(Collider a, Collider b, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (a != null && b != null)
            Physics.IgnoreCollision(a, b, false);
    }
}
