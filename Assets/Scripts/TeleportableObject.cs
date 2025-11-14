using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportableObject : MonoBehaviour
{
    public static event Action<GameObject, Transform, Transform> OnTeleport;
    public bool allowResize = true;
    public bool canBeCloned = true;

    [HideInInspector] public bool IsTeleporting = false;

    [HideInInspector] public GameObject projectionClone;
    [HideInInspector] public Transform projectionFromPortal;
    [HideInInspector] public Transform projectionToPortal;

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
        // Calcular posición relativa al portal de origen
        Vector3 localPos = fromPortal.InverseTransformPoint(transform.position);
        // Reflejar a través del plano del portal
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        // Aplicar al portal de destino CON OFFSET
        transform.position = toPortal.TransformPoint(localPos) + toPortal.forward * 0.03f;

        // Calcular rotación
        Quaternion localRot = Quaternion.Inverse(fromPortal.rotation) * transform.rotation;
        Quaternion mirror = Quaternion.Euler(0, 180, 0);
        localRot = mirror * localRot;
        transform.rotation = toPortal.rotation * localRot;

        // Ajustar velocidad si tiene Rigidbody
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 localVel = fromPortal.InverseTransformDirection(rb.linearVelocity);
            localVel = new Vector3(-localVel.x, localVel.y, -localVel.z);
            rb.linearVelocity = toPortal.TransformDirection(localVel);
        }
        
        if (allowResize)
        {
            //Scale ratio between portals
            Vector3 scaleRatio = new Vector3(
                toPortal.lossyScale.x / fromPortal.lossyScale.x,
                toPortal.lossyScale.y / fromPortal.lossyScale.y,
                toPortal.lossyScale.z / fromPortal.lossyScale.z
            );

            // Apply cumulative scale to the object
            transform.localScale = Vector3.Scale(transform.localScale, scaleRatio);
        }

        OnTeleport?.Invoke(gameObject, fromPortal, toPortal);
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

    public void CreateProjectionClone(Transform fromPortal, Transform toPortal)
    {
        if(canBeCloned == false) return;
        if (projectionClone != null) return;

        projectionFromPortal = fromPortal;
        projectionToPortal = toPortal;

        //Create the clone
        projectionClone = Instantiate(gameObject);
        projectionClone.name = gameObject.name + "_Clone";

        //Remove unnecessary components from the clone
        var cloneRb = projectionClone.GetComponent<Rigidbody>();
        if (cloneRb) Destroy(cloneRb);

        var cloneCC = projectionClone.GetComponent<CharacterController>();
        if (cloneCC) Destroy(cloneCC);

        foreach (var cam in projectionClone.GetComponentsInChildren<Camera>())
            Destroy(cam);

        var t = projectionClone.GetComponent<TeleportableObject>();
        if (t) Destroy(t);

        //Disable components on the clone
        foreach (var col in projectionClone.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    public void UpdateProjectionClone()
    {
        if (projectionClone == null) return;

        //Calculate projected pos/rot
        Vector3 localPos = projectionFromPortal.InverseTransformPoint(transform.position);
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        Vector3 projPos = projectionToPortal.TransformPoint(localPos);

        Quaternion relativeRot = Quaternion.Inverse(projectionFromPortal.rotation) * transform.rotation;
        Quaternion projRot = projectionToPortal.rotation * Quaternion.Euler(0, 180, 0) * relativeRot;

        projectionClone.transform.SetPositionAndRotation(projPos, projRot);

        //Apply scaling
        if (allowResize)
        {
            Vector3 scaleRatio = new Vector3(
                projectionToPortal.lossyScale.x / projectionFromPortal.lossyScale.x,
                projectionToPortal.lossyScale.y / projectionFromPortal.lossyScale.y,
                projectionToPortal.lossyScale.z / projectionFromPortal.lossyScale.z
            );

            projectionClone.transform.localScale = Vector3.Scale(transform.localScale, scaleRatio);
        }
    }
    public void DestroyProjectionClone()
    {
        if (projectionClone != null)
            GameObject.Destroy(projectionClone);

        //Reset references
        projectionClone = null;
        projectionFromPortal = null;
        projectionToPortal = null;
    }

}