using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalView : MonoBehaviour
{
    [Header("Portal Setup")]
    [Tooltip("The other portal's camera.")]
    public Camera sourceCamera;

    [Tooltip("The other portal's transform (the one linked to this).")]
    public Transform linkedPortal;

    [Tooltip("Reference to the player camera that looks through portals.")]
    public Camera playerCamera;

    [Header("Render Texture Settings")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public string textureName = "PortalRT";

    private RenderTexture rt;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("PortalView requires a Renderer on the same GameObject.");
            enabled = false;
            return;
        }
    }
    void OnEnable() => SetupRenderTexture();
    void OnDisable() => CleanupRenderTexture();
    void OnDestroy() => CleanupRenderTexture();

    void OnValidate()
    {
        textureWidth = Mathf.Clamp(textureWidth, 128, 4096);
        textureHeight = Mathf.Clamp(textureHeight, 128, 4096);
    }

    void LateUpdate()
    {
        if (linkedPortal == null || sourceCamera == null || playerCamera == null)
            return;

        //Compute player position relative to this portal
        Vector3 localPos = transform.InverseTransformPoint(playerCamera.transform.position);
        //Mirror through the portal plane (flip Z)
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        //Move the portal camera to the equivalent position relative to the linked portal
        sourceCamera.transform.position = linkedPortal.TransformPoint(localPos);
        //Rotate camera otation
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * playerCamera.transform.rotation;
        //Flip 180° around Y axis (like walking through the portal)
        localRot = Quaternion.Euler(0f, 180f, 0f) * localRot;
        //Apply rotation relative to linked portal
        sourceCamera.transform.rotation = linkedPortal.rotation * localRot;

        //Distance from camera to the linked portal plane, measured along portal's forward
        float distance = Vector3.Dot(linkedPortal.forward,
                                     sourceCamera.transform.position - linkedPortal.position);

        // Push near clip plane forward so it starts just beyond the linked portal surface
        sourceCamera.nearClipPlane = Mathf.Max(0.01f, distance);
    }

    void SetupRenderTexture()
    {
        if (sourceCamera == null)
        {
            Debug.LogWarning($"PortalView on '{gameObject.name}' has no sourceCamera assigned.");
            return;
        }

        //Create render texture
        rt = new RenderTexture(textureWidth, textureHeight, 24)
        {
            name = textureName + "_" + gameObject.name,
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear
        };
        rt.Create();

        sourceCamera.targetTexture = rt;

        if (rend.sharedMaterial != null)
        {
            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetTexture("_MainTex", rt);
            rend.SetPropertyBlock(mpb);
        }
        else
        {
            Debug.LogWarning($"PortalView: renderer on '{gameObject.name}' has no material assigned.");
        }
    }

    void CleanupRenderTexture()
    {
        if (sourceCamera != null && sourceCamera.targetTexture == rt)
        {
            sourceCamera.targetTexture = null;
        }
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
            rt = null;
        }
    }
}