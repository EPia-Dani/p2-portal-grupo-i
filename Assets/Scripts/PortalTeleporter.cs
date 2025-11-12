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
        if (!linkedPortal.gameObject.activeSelf) return;
        var t = other.GetComponentInParent<TeleportableObject>();
        if (t == null) return;

        float side = GetSideOfPortal(t);
        tracked[t] = side;

        //While inside the trigger, disable wall collisions so we can step through
        t.SetIgnoreWalls(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!linkedPortal.gameObject.activeSelf) return;
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
        if (!linkedPortal.gameObject.activeSelf)
        {
            return;
        }

        //Iterate a copy to modify the dict
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

            //Check crossing
            if (Mathf.Sign(currentSide) != Mathf.Sign(lastSide))
            {
                //Teleport object
                t.Teleport(portalSurface, linkedPortal);
                linkedPortal.GetComponent<PortalTeleporter>()?.NotifyIncoming(t);

                //Update tracking to prevent double triggers
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
        //Hay un problema con snapping aquí con la colisión con la pared, lo miro en otro mmomento
        //yield return null;
        yield return new WaitForSeconds(0.5f);
        obj.FinishTeleport();
    }


}