using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalView : MonoBehaviour
{
    [Tooltip("Camera that will render INTO this portal's material (the other portal's camera).")]
    public Camera sourceCamera;

    [Tooltip("Width of the RenderTexture in pixels.")]
    public int textureWidth = 1024;

    [Tooltip("Height of the RenderTexture in pixels.")]
    public int textureHeight = 1024;

    [Tooltip("Optional: name for the texture (for debugging).")]
    public string textureName = "PortalRT";

    private RenderTexture rt;
    private Renderer rend;
    private Camera oldTargetCamera; // to restore if needed

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

    void OnEnable()
    {
        SetupRenderTexture();
    }

    void OnDisable()
    {
        CleanupRenderTexture();
    }

    void OnValidate()
    {
        // Keep texture sizes reasonable and positive
        textureWidth = Mathf.Clamp(textureWidth, 128, 4096);
        textureHeight = Mathf.Clamp(textureHeight, 128, 4096);
    }

    void SetupRenderTexture()
    {
        if (sourceCamera == null)
        {
            Debug.LogWarning($"PortalView on '{gameObject.name}' has no sourceCamera assigned.");
            return;
        }

        // create render texture
        rt = new RenderTexture(textureWidth, textureHeight, 24)
        {
            name = textureName + "_" + gameObject.name,
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear
        };
        rt.Create();

        // assign to camera
        oldTargetCamera = sourceCamera.targetTexture != null ? sourceCamera : null;
        sourceCamera.targetTexture = rt;

        // assign to material
        if (rend.sharedMaterial != null)
        {
            // use material property block to avoid creating instances unnecessarily
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

    void OnDestroy()
    {
        CleanupRenderTexture();
    }
}