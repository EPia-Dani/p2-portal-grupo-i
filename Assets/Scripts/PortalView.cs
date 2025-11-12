using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalView : MonoBehaviour
{
    [Header("Portal Setup")] 
    [Tooltip("The other portal's camera.")]
    public Camera sourceCamera;
    
    public Camera _currentCamera;
    public Camera _backupCamera;

    [Tooltip("The other portal's transform (the one linked to this).")]
    public Transform linkedPortal;

    [Tooltip("Reference to the player camera that looks through portals.")]
    public Camera playerCamera;

    [Header("Render Texture Settings")]
    public int textureWidth = 1920;
    public int textureHeight = 1080;
    public string textureName = "PortalRT";
    public Texture portal_mask;

    private RenderTexture renderTarget;
    private Renderer rend;

    private bool wasEnabled = true;

    void Awake()
    {
        _currentCamera = sourceCamera;
        
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("PortalView requires a Renderer on the same GameObject.");
            enabled = false;
            return;
        }
        
        string cameraName = "Camera_"+gameObject.name;
        _backupCamera = Extensions.GetChildRecursive(cameraName, transform).GetComponent<Camera>();
    }
    void OnEnable() => SetupRenderTexture();
    void OnDisable() => CleanupRenderTexture();
    void OnDestroy() => CleanupRenderTexture();

    void OnValidate()
    {
        textureWidth = Mathf.Clamp(textureWidth, 128, 4096);
        textureHeight = Mathf.Clamp(textureHeight, 128, 4096);
    }

    void Update()
    {
        if (!sourceCamera.transform.parent.gameObject.activeSelf && wasEnabled)
        {
            wasEnabled = false;
            _currentCamera = _backupCamera;
            SetupRenderTexture();
        }
        else if (sourceCamera.transform.parent.gameObject.activeSelf && !wasEnabled)
        {
            wasEnabled = true;
            _currentCamera = sourceCamera;
            SetupRenderTexture();
        }
    }
    
    void LateUpdate()
    {
        if (linkedPortal == null || _currentCamera == null || playerCamera == null)
            return;

        //Compute player position relative to this portal
        Vector3 localPos = transform.InverseTransformPoint(playerCamera.transform.position);
        //Mirror through the portal plane (flip Z)
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        //Move the portal camera to the equivalent position relative to the linked portal
        if (sourceCamera.transform.parent.gameObject.activeSelf)
            _currentCamera.transform.position = linkedPortal.TransformPoint(localPos);
        else _currentCamera.transform.position = transform.TransformPoint(localPos);
        //Rotate camera otation
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * playerCamera.transform.rotation;
        //Flip 180� around Y axis (like walking through the portal)
        localRot = Quaternion.Euler(0f, 180f, 0f) * localRot;
        //Apply rotation relative to linked portal
        if (sourceCamera.transform.parent.gameObject.activeSelf)_currentCamera.transform.rotation = linkedPortal.rotation * localRot;
        else _currentCamera.transform.rotation = transform.rotation * localRot;

        //Distance from camera to the linked portal plane, measured along portal's forward

        float distance;
        if (linkedPortal.gameObject.activeSelf)
        {
              distance = Vector3.Dot(linkedPortal.forward,
                _currentCamera.transform.position - linkedPortal.position);
        }
        else 
        {
             distance = Vector3.Dot(transform.forward,
                _currentCamera.transform.position - transform.position);
        }
        
        // Push near clip plane forward so it starts just beyond the linked portal surface
        _currentCamera.nearClipPlane = Mathf.Max(0.01f, distance);
    }


    public void SetupRenderTexture()
    {
        if (renderTarget != null)
        {
            renderTarget.Release();
        }
        //Create render texture
        renderTarget = new RenderTexture(textureWidth, textureHeight, 0);
        //Render portal's camera to texture
        _currentCamera.targetTexture = renderTarget;

        if (rend.sharedMaterial != null)
        {
            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetTexture("_MainTex", renderTarget);
            mpb.SetTexture("_MaskTex", portal_mask);
            rend.SetPropertyBlock(mpb);
        }
        else
        {
            Debug.LogWarning($"PortalView: renderer on '{gameObject.name}' has no material assigned.");
        }
    }

    void CleanupRenderTexture()
    {
        if (_currentCamera != null && _currentCamera.targetTexture == renderTarget)
        {
            _currentCamera.targetTexture = null;
        }
        if (renderTarget != null)
        {
            renderTarget.Release();
            Destroy(renderTarget);
            renderTarget = null;
        }
    }
}