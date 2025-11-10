using UnityEditor.Rendering;
using UnityEngine;

public class Turret : MonoBehaviour
{
    
    private LineRenderer _lineRenderer;
    
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 1000f;
    
    private GameObject _currentReflector;
  
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        firePoint = Extensions.GetChildRecursive("FirePoint", transform).transform;
    }

    void Start()
    {
        ResetLaser();
    }

    void Update()
    {
        CheckForReflections();
        
    }
    
    
    private void CheckForReflections()
    {
        RaycastHit hit;
        Vector3 right = firePoint.TransformDirection(Vector3.right) * range;
        if (Physics.Raycast(firePoint.position, right, out hit, range))
        {

            if (!hit.collider.GetComponent<LaserReflection>())
            {
                _currentReflector?.GetComponent<LaserReflection>().StopCasting();
                ResetLaser();
                _currentReflector = null;
                return;
            }
            
            _lineRenderer.SetPosition(0, firePoint.localPosition);
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
            var reflector = hit.collider.GetComponent<LaserReflection>();
            _currentReflector = reflector.gameObject;
            reflector.CastLaser();
        }
        else
        {
            _currentReflector?.GetComponent<LaserReflection>().StopCasting();
            ResetLaser();
            _currentReflector = null;
        }
    }

    private void ResetLaser()
    {
        _lineRenderer.SetPosition(0, firePoint.localPosition);
        Vector3 forwardPoint = firePoint.localPosition;
        forwardPoint.z += range;
        _lineRenderer.SetPosition(1, forwardPoint);
    }
        
}
