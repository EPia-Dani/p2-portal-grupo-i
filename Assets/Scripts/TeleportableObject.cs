using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportableObject : MonoBehaviour
{
    public static event Action<GameObject> OnTeleport;
    
    [HideInInspector] public bool IsTeleporting = false;

    private Collider col;
    private Rigidbody rb;
    private CharacterController cc;
    private int wallLayer;

    void Awake()
    {
        col = GetComponentInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        wallLayer = LayerMask.NameToLayer("Wall");
    }

    public void SetIgnoreWalls(bool ignore)
    {
        if (col != null)
        {
            int mask = col.excludeLayers;
            int wallMask = 1 << wallLayer;
            if (ignore) mask |= wallMask;
            else mask &= ~wallMask;
            col.excludeLayers = mask;
        }
    }

    public void Teleport(Transform fromPortal, Transform toPortal)
    {
        if (IsTeleporting) return;
        StartCoroutine(TeleportCooldown());
        IsTeleporting = true;

        //Mirror the position relative to the portal plane
        Vector3 localPos = fromPortal.InverseTransformPoint(transform.position);
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        Vector3 newPos = toPortal.TransformPoint(localPos);

        //Mirror the rotation correctly
        Quaternion relativeRot = Quaternion.Inverse(fromPortal.rotation) * transform.rotation;
        Quaternion newRot = toPortal.rotation * Quaternion.Euler(0, 180, 0) * relativeRot;

        //Apply position and rotation
        if (cc != null)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(newPos, newRot);
            cc.enabled = true;
        }
        else if (rb != null)
        {
            // Para Rigidbody, transformar la velocidad correctamente
            Vector3 localVel = fromPortal.InverseTransformDirection(rb.linearVelocity);
            localVel = new Vector3(-localVel.x, localVel.y, -localVel.z);
            Vector3 newVel = toPortal.TransformDirection(localVel);

            rb.position = newPos;
            rb.rotation = newRot;
            rb.linearVelocity = newVel;
        }
        else
        {
            transform.SetPositionAndRotation(newPos, newRot);
        }

        SetIgnoreWalls(true);
        
        OnTeleport?.Invoke(gameObject);
    }
    public void FinishTeleport()
    {
        IsTeleporting = false;
        SetIgnoreWalls(false);
    }
    public IEnumerator TeleportCooldown()
    {
        IsTeleporting = true;
        yield return null; //Wait ONE FRAME
        IsTeleporting = false;
    }
}