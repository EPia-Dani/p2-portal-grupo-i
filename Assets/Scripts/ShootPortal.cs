using System.Collections.Generic;
using UnityEngine;

public class ShootPortal : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform orangePortal;
    public Transform bluePortal;
    public Material portalWallMaterial;
    public GameObject portalPreviewPrefab; //NO COLLIDER

    [Header("Settings")]
    public float maxPointDistance = 0.01f;
    public float maxRayDistance = 30f;
    public float placementOffset = 0.01f;
    public float resizeStep = 0.1f;
    public float minScale = 1.75f;
    public float maxScale = 7f;
    public float defaultScale = 3.5f;

    //Private class to hold the state of each portal
    private class PortalState
    {
        public Transform portal;
        public GameObject preview;
        public float scale;
        public bool holding;
        public RaycastHit lastValidHit;
        public bool hasValidHit;
    }

    private PortalState orange;
    private PortalState blue;
    private int placementLayerMask;

    private PortalState InitPortalState(Transform portal)
    {
        var state = new PortalState
        {
            portal = portal,
            preview = Instantiate(portalPreviewPrefab),
            scale = defaultScale
        };
        //Ensure preview does not block raycasts
        state.preview.layer = 2;
        state.preview.SetActive(false);
        return state;
    }

    void Start()
    {
        //Ignore preview layer
        placementLayerMask = ~(1 << 2);
        //Initialize portal states
        orange = InitPortalState(orangePortal);
        blue = InitPortalState(bluePortal);
    }

    void Update()
    {
        //Handle input for each portal
        HandlePortal(0, orange);
        HandlePortal(1, blue);
    }

    private void HandlePortal(int button, PortalState state)
    {
        if (state == null || state.preview == null || state.portal == null) return;

        bool justPressed = Input.GetMouseButtonDown(button);
        bool pressed = Input.GetMouseButton(button);
        bool justReleased = Input.GetMouseButtonUp(button);

        //Start placement of the portal
        if (justPressed)
        {

            state.scale = defaultScale; //Reset to default size
            state.holding = true;
        }

        //Handle placement of the portal
        if (pressed)
        {
            //Resize with scroll wheel
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
                state.scale = Mathf.Clamp(state.scale + scroll * resizeStep, minScale, maxScale);

            //Raycast to determine placement
            Ray ray = playerCamera.ViewportPointToRay(Vector3.one * 0.5f);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, placementLayerMask))
            {
                Vector3 pos = hit.point + hit.normal * placementOffset;
                Quaternion rot = Quaternion.LookRotation(-hit.normal, Vector3.up);

                //Preview of position and scale
                state.preview.transform.SetPositionAndRotation(pos, rot);
                state.preview.transform.localScale = Vector3.one * state.scale;

                //Check if the placement is valid
                bool valid = IsValidPosition(state.portal, pos, rot, state.scale);
                if (valid)
                {
                    state.lastValidHit = hit;
                    state.hasValidHit = true;
                }

                //Change the preview color based on validity
                var rend = state.preview.GetComponent<Renderer>();
                if (rend) rend.material.color = valid ? Color.green : Color.red;
                state.preview.SetActive(true);
            }
            else
            {
                //If there is no valid hit, hide the preview
                state.preview.SetActive(false);
            }
                
        }

        //Finalize placement of the portal
        if (justReleased)
        {
            //Hide preview
            state.preview.SetActive(false);
            //PLace portal if valid
            if (state.holding && state.hasValidHit)
            {
                Vector3 pos = state.lastValidHit.point + state.lastValidHit.normal * placementOffset;
                Quaternion rot = Quaternion.LookRotation(-state.lastValidHit.normal, Vector3.up);
                if (IsValidPosition(state.portal, pos, rot, state.scale))
                {
                    state.portal.localScale = Vector3.one * state.scale;
                    PlacePortal(state.portal, pos, rot);
                }
            }
            //Portal has been placed or placement has been cancelled
            state.holding = false;
            state.hasValidHit = false;
        }
    }

    //Check if ValidPoints are all on a wall
    private bool IsValidPosition(Transform portal, Vector3 pos, Quaternion rot, float scale)
    {
        foreach (Transform child in portal)
        {
            if (!child.name.StartsWith("ValidPoint")) continue;

            //Convert local position to world and scale
            Vector3 worldPoint = pos + rot * (child.localPosition * scale);
            Vector3 dir = (worldPoint - playerCamera.transform.position).normalized;

            //Raycast from camera to worldPoint (valid point)
            if (!Physics.Raycast(playerCamera.transform.position, dir, out RaycastHit hit, maxRayDistance, placementLayerMask))
                return false;

            //Check correct distance
            if (Vector3.Distance(hit.point, worldPoint) > maxPointDistance) return false;

            //Check correct material
            if (hit.collider.GetComponent<Renderer>()?.sharedMaterial != portalWallMaterial) return false;
        }
        return true; //All points are valid
    }

    //Place the portal
    private void PlacePortal(Transform portal, Vector3 pos, Quaternion rot)
    {
        portal.SetPositionAndRotation(pos, rot);
        if (!portal.gameObject.activeSelf) portal.gameObject.SetActive(true);
    }
}