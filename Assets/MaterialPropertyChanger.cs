using UnityEngine;

public class MaterialPropertyChanger : MonoBehaviour
{
    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    
    private Color _originalEmissionColor;

    void Start()
    {
        // Get the renderer component
        objectRenderer = GetComponent<Renderer>();
        
        // Initialize MaterialPropertyBlock for efficient property changes
        propBlock = new MaterialPropertyBlock();
        
        _originalEmissionColor = objectRenderer.sharedMaterial.GetColor("_EmissionColor");
    }

    // Method 1: Using MaterialPropertyBlock (Recommended - doesn't create material instances)
    public void ChangeColorWithPropertyBlock(Color newColor)
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", newColor);
        objectRenderer.SetPropertyBlock(propBlock);
    }

    public void ChangeEmissionWithPropertyBlock(Color emissionColor)
    {
        Color finalEmission = emissionColor;
        if (emissionColor == Color.white) finalEmission = _originalEmissionColor;
        
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", finalEmission);
        objectRenderer.SetPropertyBlock(propBlock);
    }

    public void ChangeFloatPropertyWithBlock(string propertyName, float value)
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(propertyName, value);
        objectRenderer.SetPropertyBlock(propBlock);
    }

    // Method 2: Using material.SetXXX (Creates a material instance)
    public void ChangeColorDirectly(Color newColor)
    {
        // This creates a material instance automatically
        objectRenderer.material.color = newColor;
    }

    public void ChangeTextureDirectly(Texture newTexture)
    {
        objectRenderer.material.mainTexture = newTexture;
    }

    public void ChangeMetallicDirectly(float metallic)
    {
        objectRenderer.material.SetFloat("_Metallic", metallic);
    }

    public void ChangeSmoothnessDirectly(float smoothness)
    {
        objectRenderer.material.SetFloat("_Glossiness", smoothness);
    }

    // Method 3: Using sharedMaterial (Affects all objects using this material)
    public void ChangeSharedMaterialColor(Color newColor)
    {
        // WARNING: This changes the material for ALL objects using it
        objectRenderer.sharedMaterial.color = newColor;
    }

    // Example: Animated color change
    void Update()
    {
        // Uncomment to see animated color changes
        // float t = Mathf.PingPong(Time.time, 1f);
        // Color animatedColor = Color.Lerp(Color.red, Color.blue, t);
        // ChangeColorWithPropertyBlock(animatedColor);
    }

    // Example: Change multiple properties at once
    public void ChangeMultipleProperties(Color color, float metallic, float smoothness)
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", color);
        propBlock.SetFloat("_Metallic", metallic);
        propBlock.SetFloat("_Glossiness", smoothness);
        objectRenderer.SetPropertyBlock(propBlock);
    }

    // Clean up material instances to avoid memory leaks
    void OnDestroy()
    {
        if (objectRenderer != null && objectRenderer.material != null)
        {
            // Destroy the material instance if it was created
            Destroy(objectRenderer.material);
        }
    }
}
