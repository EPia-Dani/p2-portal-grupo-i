using UnityEditor.Rendering;
using UnityEngine;

public class Turret : MonoBehaviour
{
    
    private LineRenderer _lineRenderer;
    
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 1000f;
  
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        firePoint = Extensions.GetChildRecursive("FirePoint", transform).transform;
    }

    void Start()
    {
        _lineRenderer.SetPosition(0, firePoint.localPosition);
        Vector3 forwardPoint = firePoint.localPosition;
        forwardPoint.z += range;
        _lineRenderer.SetPosition(1, forwardPoint);
    }

    void Update()
    {
        CheckForCollisions();
        
    }
    
    
    private void CheckForCollisions()
    {
        RaycastHit hit;
        Vector3 right = firePoint.TransformDirection(Vector3.right) * range;
        if (Physics.Raycast(firePoint.position, right, out hit, range))
        {
            Debug.Log("Turret hit: " + hit.collider.name);
            Debug.DrawRay(firePoint.position, right, Color.green);
            
        }
    }
}
