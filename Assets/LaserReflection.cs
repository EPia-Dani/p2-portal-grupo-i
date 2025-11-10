using UnityEngine;

public class LaserReflection : MonoBehaviour
{
    [SerializeField] private float range = 1500f;
    
    private Transform firePoint => Extensions.GetChildRecursive("FirePoint", transform).transform;
    private GameObject _currentReflector;
    
    private LineRenderer _lineRenderer;
    
    private Vector3 _castingDirection;
    
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        StopCasting();

    }

    public void CastLaser(Vector3 direction)
    {
    
        Vector3 surfaceNormal = transform.forward;
        Vector3 reflectedDirection = Vector3.Reflect(direction, surfaceNormal);
        
        reflectedDirection = Quaternion.AngleAxis(90f, transform.up) * reflectedDirection;
    
        _lineRenderer.SetPosition(0, firePoint.localPosition);
        _castingDirection = reflectedDirection;
        
        CheckForReflections();
        
        
    }
    
    public void StopCasting()
    {
        if (_currentReflector)
        {
            _currentReflector.GetComponent<LaserReflection>().StopCasting();
            _currentReflector = null;
        }
        _lineRenderer.SetPosition(0, firePoint.localPosition);
        _lineRenderer.SetPosition(1, Vector3.zero);
        _castingDirection = Vector3.zero;
    }

    private void Update()
    { 
    }

    private void CheckForReflections()
    {
        Ray reflectedRay = new Ray(firePoint.position, _castingDirection);

        if (Physics.Raycast(reflectedRay, out RaycastHit hit, range) && _castingDirection != Vector3.zero)
        {
            var reflector = hit.collider.GetComponent<LaserReflection>();

            if (reflector && !reflector.IsReflecting())
            {
                reflector.CastLaser(_castingDirection);
                _currentReflector = reflector.gameObject;
            }
            else if (_currentReflector)
            {
                _currentReflector.GetComponent<LaserReflection>().StopCasting();
                _currentReflector = null;
            }

            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
        }
        else
        {
            Vector3 forwardPoint = firePoint.localPosition + transform.InverseTransformDirection(_castingDirection.normalized * range);
            _lineRenderer.SetPosition(1, forwardPoint);
        }

    }
    
    public bool IsReflecting()
    {
        return _currentReflector == true;
    }
}
