
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
            var reflector = hit.collider.GetComponent<LaserReflection>();
            
            if (reflector)
            {
                if (reflector.IsReflecting() && reflector.gameObject != _currentReflector)
                {
                    _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
                    return;
                }
                
                if(reflector.gameObject != _currentReflector && _currentReflector)
                {
                    _currentReflector.GetComponent<LaserReflection>().StopCasting();
                    _currentReflector = reflector.gameObject;
                }
                else if (reflector.gameObject != _currentReflector)
                {
                    _currentReflector = reflector.gameObject;
                }
                reflector.CastLaser(hit.point, right.normalized, hit.normal);
                
            }
            else
            {
                if (_currentReflector)
                {
                    _currentReflector.GetComponent<LaserReflection>().StopCasting();
                    _currentReflector = null;
                }
            }
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
        }
        else
        {
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(firePoint.position + right*range));
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
