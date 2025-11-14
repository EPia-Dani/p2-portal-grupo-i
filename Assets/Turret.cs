
using UnityEngine;

public class Turret : MonoBehaviour
{
    
    [SerializeField] private Transform firePoint;
    [SerializeField] private float range = 1000f;
    [SerializeField] private float damageTick = 20f;
    [SerializeField] private AudioClip deathSound;
    
    
    private LineRenderer _lineRenderer;
    private Grabbable _grabbable;
    private AudioManager _audioManager;
    
    private GameObject _currentImpact;
    private GameObject _particleSystem;

    private float _lastPlayerHitTime = -0.5f;
    private bool _isDead = false;
    
  
    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _grabbable = GetComponent<Grabbable>();
        _audioManager = GetComponent<AudioManager>();
    }

    void Start()
    {
        firePoint = Extensions.GetChildRecursive("FirePoint", transform).transform;
        _particleSystem = Extensions.GetChildRecursive("ParticleSystem", transform).gameObject;
        _particleSystem.SetActive(false);
        ResetLaser();
        
    }
    

    void Update()
    {
        var wasDead = _isDead;
        if(!_isDead) _isDead = CheckIsDead();
        
        if(!_isDead) CheckForImpacts();
        
        else
        {
            if(!wasDead) _audioManager.PlaySfx(deathSound, 1f);
            StopImpact();
            StopCasting();
        }
    }
    
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void CheckForImpacts()
    {
        RaycastHit hit;
        Vector3 right = firePoint.TransformDirection(Vector3.right) * range;

        if (Physics.Raycast(firePoint.position, right, out hit, range))
        {
            var reflector = hit.collider.GetComponent<LaserReflection>();
            var receiver = hit.collider.GetComponent<LaserReceiver>();
            var player = hit.collider.GetComponent<PlayerStatusManager>();
            var turret = hit.collider.GetComponent<Turret>();
            
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
                else if (reflector.gameObject && reflector.gameObject != _currentImpact)
                {
                    _currentImpact = reflector.gameObject;
                }
                reflector.CastLaser(hit.point, right.normalized, hit.normal);
                
                _particleSystem.SetActive(false);
                
            }
            else if (receiver)
            {

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
        _lineRenderer.positionCount = 2;
        
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
    
    public bool CheckIsDead()
    {
        if (!_grabbable.IsGrabbed())
        {
            return Vector3.Dot(transform.up, Vector3.up) < 0.7f;
        }
        return false;
    }

    private void StopCasting()
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
    
    public void Revive()
    {
        if (_isDead)
        {
            _isDead = false;
            ResetLaser();
        }
    }
    
    public void Kill()
    {
        if(!_isDead) _audioManager.PlaySfx(deathSound, 1f);
        StopImpact();
        StopCasting();
        
        _isDead = true;
        GetComponent<Rigidbody>().AddForce(transform.up * 0.5f, ForceMode.VelocityChange);
    }
        
}
