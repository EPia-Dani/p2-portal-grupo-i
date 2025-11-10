using UnityEngine;

public class LaserReflection : MonoBehaviour
{
    [SerializeField] private float range = 1500f;
    
    private LineRenderer _lineRenderer;
    
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        StopCasting();

    }

    public void CastLaser()
    {
        _lineRenderer.SetPosition(1, Vector3.forward * range);
    }
    
    public void StopCasting()
    {
        _lineRenderer.SetPosition(1, Vector3.zero);
    }
}
