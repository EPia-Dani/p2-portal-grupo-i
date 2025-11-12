using System.Collections.Generic;
using UnityEngine;

public class ShootPortal : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform orangePortal;
    public Transform bluePortal;
    public Material portalWallMaterial;

    [Header("Settings")]
    public float maxPointDistance = 0.01f;
    public float maxNormalAngleDeg = 30f;
    public float maxRayDistance = 30f;
    public float placementOffset = 0.01f;

    private void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            return;

        Vector3 pos = hit.point + hit.normal * placementOffset;
        Quaternion rot = Quaternion.LookRotation(-hit.normal, Vector3.up);

        //Check placement validity for each portal type
        

        if (Input.GetMouseButtonDown(0))
        {
            bool orangeValid = IsValidPosition(orangePortal, pos, rot);
            if(orangeValid) PlacePortal(orangePortal, pos, rot);
        }

        if (Input.GetMouseButtonDown(1))
        {
            bool blueValid = IsValidPosition(bluePortal, pos, rot);
            if(blueValid) PlacePortal(bluePortal, pos, rot);
        }
    }

    private bool IsValidPosition(Transform portal, Vector3 pos, Quaternion rot)
    {
        //Get ValidPoints
        List<Transform> points = new();
        foreach (Transform child in portal)
        {
            Debug.Log(child.name + "node" + child.transform.localPosition);
            if (child.name.StartsWith("ValidPoint"))
                points.Add(child);
        }

        Vector3 camPos = playerCamera.transform.position;

        foreach (Transform child in points)
        {
            // Obtener la posición local del punto relativo a su padre (portal)
            Vector3 localPos = child.localPosition;

            // Aplicar la escala del portal a la posición local antes de transformar
            Vector3 scaledLocalPos = Vector3.Scale(localPos, portal.localScale);

            // Transformar al espacio mundial usando pos y rot del raycast
            Vector3 worldPoint = pos + rot * scaledLocalPos;

            Debug.Log($"{child.name} - Local: {localPos}, Scaled: {scaledLocalPos}, World: {worldPoint}");

            // Dirección desde la cámara al punto mundial
            Vector3 direction = (worldPoint - camPos).normalized;

            if (!Physics.Raycast(camPos, direction, out RaycastHit hit, maxRayDistance))
                return false;

            Debug.DrawLine(camPos, hit.point, Color.green, 5f);

            // Verificar distancia, normal y material si es necesario
            float distance = Vector3.Distance(hit.point, worldPoint);
            if (distance > maxPointDistance)
            {
                Debug.Log($"{child.name} - Distance check failed: {distance} > {maxPointDistance}");
                return false;
            }

            Material hitMaterial = hit.collider.gameObject.GetComponent<Renderer>()?.sharedMaterial;
            if (hitMaterial != portalWallMaterial)
            {
                Debug.Log($"{child.name} - Material check failed: {hitMaterial?.name ?? "null"} != {portalWallMaterial?.name ?? "null"}");
                return false;
            }
        }

        return true;
    }

    private void PlacePortal(Transform portal, Vector3 pos, Quaternion rot)
    {
        portal.SetPositionAndRotation(pos, rot);
        if(!portal.gameObject.activeSelf)
            portal.gameObject.SetActive(true);
    }
}