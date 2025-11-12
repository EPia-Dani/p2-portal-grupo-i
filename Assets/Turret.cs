
using UnityEngine;

public class Turret : MonoBehaviour
{
    
    private LineRenderer _lineRenderer;
    
    private GameObject _particleSystem;
    
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 1000f;
    
    private GameObject _currentImpact;
  
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        firePoint = Extensions.GetChildRecursive("FirePoint", transform).transform;
    }

    void Start()
    {
        _particleSystem = Extensions.GetChildRecursive("ParticleSystem", transform).gameObject;
        _particleSystem.SetActive(false);
        ResetLaser();
        
    }

    void Update()
    {
        CheckForImpacts();
    }
    
    
    private void CheckForImpacts()
    {
        RaycastHit hit;
        Vector3 right = firePoint.TransformDirection(Vector3.right) * range;

        if (Physics.Raycast(firePoint.position, right, out hit, range))
        {
            var reflector = hit.collider.GetComponent<LaserReflection>();
            var receiver = hit.collider.GetComponent<LaserReceiver>();
            
            if (reflector)
            {
                if (reflector.IsReflecting() && reflector.gameObject != _currentImpact)
                {
                    _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
                    return;
                }
                
                if(reflector.gameObject != _currentImpact && _currentImpact)
                {
                    _currentImpact.GetComponent<LaserReflection>().StopCasting();
                    _currentImpact = reflector.gameObject;
                }
                else if (reflector.gameObject != _currentImpact)
                {
                    _currentImpact = reflector.gameObject;
                }
                reflector.CastLaser(hit.point, right.normalized, hit.normal);
                
                _particleSystem.SetActive(false);
                
            }
            else if (receiver)
            {
                Debug.Log("Hit " + hit.collider.name);

                if (receiver.IsTriggered()) return;
                receiver.Trigger(true);
                _particleSystem.SetActive(false);
                _currentImpact = receiver.gameObject;
                
            }
            else
            {
                Debug.Log("Hit " + hit.collider.name);
                if (!_particleSystem.activeSelf)
                    _particleSystem.SetActive(true);
                _particleSystem.transform.position = hit.point;
                _particleSystem.transform.rotation = Quaternion.LookRotation(hit.normal);
                
                StopImpact();
            }
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(hit.point));
        }
        else
        {
            StopImpact();
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(firePoint.position + right*range));
            _particleSystem.SetActive(false);
        }
    }

    private void ResetLaser()
    {
        _lineRenderer.SetPosition(0, firePoint.localPosition);
        Vector3 forwardPoint = firePoint.localPosition;
        forwardPoint.z += range;
        _lineRenderer.SetPosition(1, forwardPoint);
    }
    
    private void StopImpact()
    {
        if (_currentImpact)
        {
            var currentReceiver = _currentImpact.GetComponent<LaserReceiver>();
            var currentReflector = _currentImpact.GetComponent<LaserReflection>();
            if (currentReceiver != null)
            {
                Debug.Log("Stopping impact on " + currentReceiver.name);
                currentReceiver.Trigger(false);
            }
            else if (currentReflector != null)
            {
                currentReflector.StopCasting();
            }
                    
            _currentImpact = null;
        }
    }
        
}
