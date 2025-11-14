using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportableObject : MonoBehaviour
{
    public static event Action<GameObject, Transform, Transform> OnTeleport;
    public bool allowResize = true;

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
        if (IsTeleporting || rb == null) return;
        StartCoroutine(TeleportCooldown());
        IsTeleporting = true;

        // Transformar posición
        Vector3 localPos = fromPortal.InverseTransformPoint(transform.position);
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        Vector3 newPos = toPortal.TransformPoint(localPos);

        // Transformar velocidad completa al espacio local del portal de entrada
        Vector3 localVel = fromPortal.InverseTransformDirection(rb.linearVelocity);

        // Aplicar transformación espejo (invertir X y Z)
        localVel = new Vector3(-localVel.x, localVel.y, -localVel.z);

        // Convertir al espacio mundial del portal de salida
        Vector3 newVel = toPortal.TransformDirection(localVel);

        // Transformar rotación
        Quaternion localRot = Quaternion.Inverse(fromPortal.rotation) * transform.rotation;
        Quaternion mirror = Quaternion.Euler(0, 180, 0);
        localRot = mirror * localRot;
        Quaternion newRot = toPortal.rotation * localRot;

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


        // Desactivar temporalmente interpolación para evitar conflictos
        RigidbodyInterpolation prevInterpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;

        // Aplicar transformaciones al Rigidbody
        rb.position = newPos;
        rb.rotation = newRot;
        rb.linearVelocity = newVel;

        Physics.SyncTransforms();

        // Restaurar interpolación
        rb.interpolation = prevInterpolation;

        SetIgnoreWalls(true);
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
}