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
        bool orangeValid = IsValidPosition(orangePortal, pos, rot);
        bool blueValid = IsValidPosition(bluePortal, pos, rot);

        if (orangeValid && Input.GetMouseButtonDown(0))
            PlacePortal(orangePortal, pos, rot);

        if (blueValid && Input.GetMouseButtonDown(1))
            PlacePortal(bluePortal, pos, rot);
    }

    private bool IsValidPosition(Transform portal, Vector3 pos, Quaternion rot)
    {
        //Get ValidPoints
        List<Transform> points = new();
        foreach (Transform child in portal)
        {
            if (child.name.StartsWith("ValidPoint"))
                points.Add(child);
        }

        Vector3 camPos = playerCamera.transform.position;

        foreach (Transform p in points)
        {
            Vector3 localPos = p.localPosition;
            Quaternion localRot = p.localRotation;

            Vector3 worldPoint = pos + rot * localPos;
            Vector3 dir = (worldPoint - camPos);
            float dist = dir.magnitude;
            if (dist < 0.0001f)
                return false;

            dir /= dist;

            //Ray from camera to point
            if (!Physics.Raycast(camPos, dir, out RaycastHit hit, Mathf.Min(maxRayDistance, dist + 0.1f)))
                return false;

            //Check Distance
            if (Vector3.Distance(hit.point, worldPoint) > maxPointDistance)
                return false;

            //Check Normal angle
            Vector3 expectedForward = rot * localRot * Vector3.forward;
            if (Vector3.Angle(hit.normal, expectedForward) > maxNormalAngleDeg)
                return false;

            //Check wall material
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null || rend.sharedMaterial != portalWallMaterial)
                return false;
        }

        return true;
    }

    private void PlacePortal(Transform portal, Vector3 pos, Quaternion rot)
    {
        portal.SetPositionAndRotation(pos, rot);
    }
}