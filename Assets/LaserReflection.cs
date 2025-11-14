using System;
using UnityEngine;

public class LaserReflection : MonoBehaviour
{
    [SerializeField] private float range = 1500f;

    private GameObject _currentImpact;
    private LineRenderer _lineRenderer;
    [SerializeField] private float damageTick = 10f;
    private float _lastPlayerHitTime = -0.5f;
    
    private GameObject _particleSystem;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        
        if (_lineRenderer != null)
            _lineRenderer.useWorldSpace = true;
            
        
    }

    private void Start()
    {
        
        _particleSystem = Extensions.GetChildRecursive("ParticleSystem", transform).gameObject;
        StopCasting();
    }

    public void CastLaser(Vector3 hitPoint, Vector3 incomingDirection, Vector3 hitNormal)
    {
        if (_lineRenderer == null) return;
        
        Vector3 reflectedDirection = Vector3.Reflect(incomingDirection.normalized, hitNormal.normalized);
        Vector3 origin = hitPoint + reflectedDirection * 0.01f;
        
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, hitPoint);
        

        int glassLayer = LayerMask.NameToLayer("Glass");
        int layerMask = ~(1 << glassLayer);
        
        if (Physics.Raycast(origin, reflectedDirection, out RaycastHit hit, range, layerMask))
        {
            _lineRenderer.SetPosition(1, hit.point);

            var reflector = hit.collider.GetComponent<LaserReflection>();
            var receiver = hit.collider.GetComponent<LaserReceiver>();
            var player = hit.collider.GetComponent<PlayerStatusManager>();
            var turret = hit.collider.GetComponent<Turret>();
            
            if (reflector)
            {
                if (reflector.IsReflecting() && reflector.gameObject != _currentImpact)
                {
                    return;
                }

                if(reflector.gameObject != _currentImpact && _currentImpact)
                {
                    _currentImpact.GetComponent<LaserReflection>().StopCasting();
                }
                
                _currentImpact = reflector.gameObject;
                reflector.CastLaser(hit.point, reflectedDirection, hit.normal);
                
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
            else if (turret)
            {
                if (turret.CheckIsDead()) return;
                turret.Kill();
                _particleSystem.SetActive(false);
                _currentImpact = turret.gameObject;
            }
            else
            {
                
                if (!_particleSystem.activeSelf)
                    _particleSystem.SetActive(true);
                _particleSystem.transform.position = hit.point;
                _particleSystem.transform.rotation = Quaternion.LookRotation(hit.normal);
                
                if(player)
                {
                    if (Time.time - _lastPlayerHitTime >= 1f)
                    {
                        player.TakeDamage(damageTick);
                        _lastPlayerHitTime = Time.time;
                    }
                }
                
                
                StopImpact();
            }
        }
        else
        {
            StopImpact();
            
            Vector3 endPoint = origin + reflectedDirection * range;
            _lineRenderer.SetPosition(1, endPoint);
        }
    }

    public void StopCasting()
    {
        if (_currentImpact != null)
        {
            _currentImpact.GetComponent<LaserReflection>()?.StopCasting();
            _currentImpact = null;
        }

        if (_lineRenderer != null)
            _lineRenderer.positionCount = 0;
        
        _particleSystem.SetActive(false);
    }

    public bool IsReflecting()
    {
        return _lineRenderer != null && _lineRenderer.positionCount > 0;
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