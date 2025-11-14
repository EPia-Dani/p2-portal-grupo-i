using System;
using System.Collections;
using System.Diagnostics.SymbolStore;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    private Camera _playerCamera;
    private GameObject _firingPoint;
    private Animator _animator;

    private float _lastTimeShot;
    public float roundsPerMinute = 200;
    private float _shootInterval;
    
    [SerializeField] public int _currentAmmo;
    [SerializeField] public int _maxAmmo;
    [SerializeField] private int _damagePerShot = 40;
    
    [SerializeField] private AudioClip gunShotAudioClip;
    [SerializeField] private AudioClip reloadAudioClip;

    private float _shotDelay = 0.01f;


    public GameObject texture;

    public static event Action<int, int> WeaponShot;
    public static event Action<RaycastHit> BulletShot;
    public static event Action<GameObject, int> EnemyShot;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _shootInterval = 60 / roundsPerMinute;
        
        _currentAmmo = _maxAmmo;
        

    }

    private void Awake()
    {
        _playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).gameObject.GetComponent<Camera>();
        _firingPoint = Extensions.GetChildRecursive("FiringPoint", this.transform);
    }
    
    private void OnEnable()
    {
    }
    
    private void OnDisable()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        _shootInterval = 60 / roundsPerMinute;
    }

    public void Fire(bool performed)
    {
        if (performed) StartCoroutine(nameof(TryShoot));
    }

    public IEnumerator TryShoot()
    {

        if (((_lastTimeShot==0) || (_lastTimeShot + _shootInterval < Time.time)) && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Reload") && _currentAmmo > 0)
        {
            yield return new WaitForSeconds(_shotDelay);

            _animator.SetTrigger("Shoot");
            _lastTimeShot = Time.time;
            _currentAmmo--;
            AudioManager.instance.PlaySfx(gunShotAudioClip, 0.7f);
            WeaponShot?.Invoke(_currentAmmo, _maxAmmo);

            var hit = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));


            if (Physics.Raycast(hit, out var hitInfo))
            {
                var destination = hitInfo.point;
                var direction = (destination - _firingPoint.transform.position).normalized;

                var shot = new Ray(_firingPoint.transform.position, direction);

                if (!Physics.Raycast(shot, out hitInfo)) yield return null;

                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
                {
                    BulletShot?.Invoke(hitInfo);
                }
                if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                {
                    EnemyShot?.Invoke(hitInfo.collider.gameObject, _damagePerShot);
                }
            }
        }
    }

    public void Reload(bool performed)
    {
        if (performed && _currentAmmo < _maxAmmo)
        {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
            {
                _animator.SetTrigger("Reload");
                AudioManager.instance.PlaySfx(reloadAudioClip, 0.7f);
            }
            
        }
    }

    public void InsertAmmo()
    {
        _currentAmmo = _maxAmmo;
    }

    private void KillAnimations()
    {
        _animator.SetTrigger("Death");
    }
}