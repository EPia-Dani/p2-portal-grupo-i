using System;
using Interactable;
using UnityEngine;

public class FPSCharacterController : MonoBehaviour
{
    public float horizontalSpeed = 10f;
    public float runningSpeed = 20f;
    public float jumpForce = 10f;
    public float interactionDistance = 3f;
    public float grabDistance = 6f;

    public float groundCheckDistance = 0.2f;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    //WEAPON LEANING
    public float _weaponZLean = 0;
    public float _weaponXLean = 0;
    public float leanZAngle = 15f;
    public float leanXAngle = 15f;
    public float baseLeaning = 0f;
    public float leaningSpeed = 2f;

    private Camera _playerCamera;
    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private AudioSource _footstepAudioSource;
    public GameObject arms;

    private float _movementSpeed;

    private bool _isRunning;
    private bool _isCrouched;
    private bool _isGrounded;
    private bool _hasGrabbed;

    private Vector2 _direction;

    public static event Action<GameObject> onGrab;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _footstepAudioSource = GetComponent<AudioSource>();
    
        // Configuración del Rigidbody sin interpolación
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.useGravity = true;
        _rigidbody.mass = 1f;
        _rigidbody.linearDamping = 0f;
        _rigidbody.angularDamping = 0.05f;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.interpolation = RigidbodyInterpolation.None; // Cambio crítico

        arms = Extensions.GetChildRecursive("Arms", this.transform);
        _playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).GetComponent<Camera>();
    }

    void Update()
    {
        CheckGrounded();
        
        _movementSpeed = _isRunning ? runningSpeed : horizontalSpeed;

        bool shouldPlayFootsteps = _isGrounded && _direction.magnitude > 0.1f;
    
        if (_footstepAudioSource != null)
        {
            if (shouldPlayFootsteps && !_footstepAudioSource.isPlaying)
            {
                _footstepAudioSource.Play();
            }
            else if (!shouldPlayFootsteps && _footstepAudioSource.isPlaying)
            {
                _footstepAudioSource.Stop();
            }
        }
        
        WeaponLeaning();
        arms.transform.localRotation = Quaternion.Euler(_weaponXLean, 0, _weaponZLean);
    }

    void FixedUpdate()
    {
        if (_isGrounded)
        {
            // Solo controlar movimiento cuando está en el suelo
            Vector3 horizontalVelocity = (transform.forward * _direction.y + transform.right * _direction.x) * _movementSpeed;
            _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }
        else
        {
            // En el aire, solo aplicar control parcial (air control)
            Vector3 airControl = (transform.forward * _direction.y + transform.right * _direction.x) * (horizontalSpeed * 0.3f);
            Vector3 currentHorizontal = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, currentHorizontal + airControl, Time.fixedDeltaTime * 2f);
        
            _rigidbody.linearVelocity = new Vector3(newHorizontal.x, _rigidbody.linearVelocity.y, newHorizontal.z);
        }
    }

    void CheckGrounded()
    {
        // Usar SphereCast desde el centro del collider hacia abajo
        float distanceToGround = (_capsuleCollider.height / 2f) - _capsuleCollider.radius;
        Vector3 spherePosition = transform.position - new Vector3(0, distanceToGround, 0);

        // Usar ~0 para incluir todas las capas
        _isGrounded = Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);

        // Debug visual
        Debug.DrawRay(spherePosition, Vector3.down * groundCheckDistance, _isGrounded ? Color.green : Color.red);
    }

    void WeaponLeaning()
    {
        if (_isGrounded)
        {
            switch (_direction.magnitude)
            {
                case 0 when _weaponZLean != 0:
                {
                    if (_weaponZLean is < 0.1f and > -0.1f) _weaponZLean = 0;
                    else _weaponZLean = Mathf.Lerp(_weaponZLean, 0, Time.deltaTime * leaningSpeed);
                    break;
                }
                case 0 when _weaponXLean != 0:
                {
                    if (_weaponXLean < 0.1f && _weaponXLean > 0.1f) _weaponXLean = 0;
                    else _weaponXLean = Mathf.Lerp(_weaponXLean, baseLeaning, Time.deltaTime * leaningSpeed);
                    break;
                }
                case > 0:
                    _weaponZLean = Mathf.Lerp(_weaponZLean, -_direction.x * leanZAngle, Time.deltaTime * leaningSpeed);
                    _weaponXLean = Mathf.Lerp(_weaponXLean, -_direction.y * leanXAngle, Time.deltaTime * leaningSpeed);
                    break;
            }
        }
    }

    public void HandleInteraction(bool performed)
    {
        var excludedLayers = ~LayerMask.GetMask("Player");

        if (performed)
        {
            var ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.yellow);

            if (_hasGrabbed)
            {
                onGrab?.Invoke(null);
                _hasGrabbed = false;
            }
            else if (Physics.Raycast(ray, out hit, interactionDistance, excludedLayers))
            {
                Debug.Log("Impacted " + hit.collider.gameObject.layer);

                if(hit.collider.tag.Contains("Interactable"))
                {
                    if (hit.distance < interactionDistance)
                    {
                        var interactable = hit.collider.GetComponent<IInteractable>();
                        interactable?.Interact();
                    }
                }
                if (hit.collider.tag.Contains("Grabbable"))
                {
                    if (Vector3.Distance(hit.collider.transform.position, transform.position) > grabDistance)
                    {
                        return;
                    }
                    if (!_hasGrabbed) _hasGrabbed = true;

                    onGrab?.Invoke(hit.collider.gameObject);
                }
            }
        }
    }

    public bool GetGrounded()
    {
        return _isGrounded;
    }

    public bool GetRunning()
    {
        return _isRunning;
    }

    public void Move(Vector2 direction)
    {
        _direction = direction;
    }

    public void Jump(bool performed)
    {
        if(!_isGrounded || !performed)
        {
            return;
        }

        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Sprint(bool performed)
    {
        if(_isGrounded) _isRunning = performed;
    }

    public void Crouch(bool performed)
    {

    }

    public float GetMovementSpeed()
    {
        return _movementSpeed;
    }

    public Vector2 GetMovementDirection()
    {
        return _direction;
    }

    private void KillMovement()
    {
        _direction = Vector2.zero;
        _movementSpeed = 0f;
        _rigidbody.linearVelocity = Vector3.zero;
    }
}