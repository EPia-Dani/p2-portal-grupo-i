using System;
using UnityEngine;

public class LaserReflection : MonoBehaviour
{
    [SerializeField] private float range = 1500f;

    private GameObject _currentReflector;
    private LineRenderer _lineRenderer;
    
    private ParticleSystem _particleSystem;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        
        if (_lineRenderer != null)
            _lineRenderer.useWorldSpace = true;
            
        StopCasting();
    }

    private void Start()
    {
        _particleSystem = Extensions.GetChildRecursive("ParticleSystem", transform).gameObject.GetComponent<ParticleSystem>();
        _particleSystem.Stop();
    }

    public void CastLaser(Vector3 hitPoint, Vector3 incomingDirection, Vector3 hitNormal)
    {
        if (_lineRenderer == null) return;
        
        Vector3 reflectedDirection = Vector3.Reflect(incomingDirection.normalized, hitNormal.normalized);
        Vector3 origin = hitPoint + reflectedDirection * 0.01f;
        
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, hitPoint);
        

        if (Physics.Raycast(origin, reflectedDirection, out RaycastHit hit, range))
        {
            _lineRenderer.SetPosition(1, hit.point);

            var reflector = hit.collider.GetComponent<LaserReflection>();

            if (reflector)
            {
                if (reflector.IsReflecting() && reflector.gameObject != _currentReflector)
                {
                    return;
                }

                if(reflector.gameObject != _currentReflector && _currentReflector)
                {
                    _currentReflector.GetComponent<LaserReflection>().StopCasting();
                }
                
                _currentReflector = reflector.gameObject;
                reflector.CastLaser(hit.point, reflectedDirection, hit.normal);
                
                _particleSystem.Stop();
                
            }
            else
            {
                
                if (!_particleSystem.isPlaying)
                    _particleSystem.Play();
                _particleSystem.transform.position = hit.point;
                _particleSystem.transform.rotation = Quaternion.LookRotation(hit.normal);
                
                if (_currentReflector)
                {
                    _currentReflector.GetComponent<LaserReflection>().StopCasting();
                    _currentReflector = null;
                }
            }
        }
        else
        {
            _particleSystem.Stop();
            
            Vector3 endPoint = origin + reflectedDirection * range;
            _lineRenderer.SetPosition(1, endPoint);
        }
    }

    public void StopCasting()
    {
        if (_currentReflector != null)
        {
            _currentReflector.GetComponent<LaserReflection>()?.StopCasting();
            _currentReflector = null;
        }

        if (_lineRenderer != null)
            _lineRenderer.positionCount = 0;
    }

    public bool IsReflecting()
    {
        return _lineRenderer != null && _lineRenderer.positionCount > 0;
    }
}