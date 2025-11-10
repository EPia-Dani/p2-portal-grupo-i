using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class PortalTeleporter : MonoBehaviour
{
    [Header("Portal Links")]
    public Transform portalSurface;
    public Transform linkedPortal;

    private readonly Dictionary<TeleportableObject, float> tracked = new();

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"{name}: PortalTeleporter collider should be a trigger.");
        if (portalSurface == null)
            portalSurface = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponentInParent<TeleportableObject>();
        if (t == null) return;

        float side = GetSideOfPortal(t);
        tracked[t] = side;

        // While inside the trigger, disable wall collisions so we can step through
        t.SetIgnoreWalls(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var t = other.GetComponentInParent<TeleportableObject>();
        if (t == null) return;

        if (tracked.ContainsKey(t))
        {
            tracked.Remove(t);
            t.FinishTeleport();
        }
    }

    private void Update()
    {
        if (linkedPortal == null) return;

        // Iterate a copy so we can modify the dict
        foreach (var pair in new Dictionary<TeleportableObject, float>(tracked))
        {
            TeleportableObject t = pair.Key;
            if (t == null)
            {
                tracked.Remove(t);
                continue;
            }

            float lastSide = pair.Value;
            float currentSide = GetSideOfPortal(t);

            // Visual debug to confirm correct portal orientation
            Debug.DrawLine(portalSurface.position, portalSurface.position + portalSurface.forward * 0.5f, Color.cyan);

            // Has it crossed from one side of the portal plane to the other?
            if (Mathf.Sign(currentSide) != Mathf.Sign(lastSide))
            {
                // Center crossed — teleport!
                t.Teleport(portalSurface, linkedPortal);
                linkedPortal.GetComponent<PortalTeleporter>()?.NotifyIncoming(t);

                // Update tracking to prevent double triggers
                tracked[t] = currentSide;
            }
            else
            {
                tracked[t] = currentSide;
            }
        }
    }

    private float GetSideOfPortal(TeleportableObject obj)
    {
        var c = obj.GetComponentInChildren<Collider>();
        Vector3 center = c ? c.bounds.center : obj.transform.position;
        Vector3 offset = center - portalSurface.position;
        return Vector3.Dot(portalSurface.forward, offset);
    }

    public void NotifyIncoming(TeleportableObject obj)
    {
        StartCoroutine(FinishAfterFrame(obj));
    }

    private System.Collections.IEnumerator FinishAfterFrame(TeleportableObject obj)
    {
        //yield return null;
        yield return new WaitForSeconds(0.5f);
        obj.FinishTeleport();
    }


}