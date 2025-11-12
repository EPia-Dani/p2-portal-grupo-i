using System;
using Interactable;
using UnityEngine;

public class FPSCharacterController : MonoBehaviour
{
    public float horizontalSpeed = 10f;
    public float runningSpeed = 20f;
    public float jumpSpeed = 10f;
    public float interactionDistance = 3f;
    public float grabDistance = 6f;
    
    public float gravityMultiplier = 2f;
    
    //WEAPON LEANING
    public float _weaponZLean = 0;
    public float _weaponXLean = 0;
    public float leanZAngle = 15f;
    public float leanXAngle = 15f;
    public float baseLeaning = 0f;
    public float leaningSpeed = 2f;
    
    private Camera _playerCamera;
    public CharacterController characterController;
    public GameObject arms;
    

    private float _movementSpeed;
    private float _verticalSpeed;
    
    private bool _isRunning;
    private bool _isCrouched;
    private bool _isGrounded;
    private bool _hasGrabbed;
    
    private Vector2 _direction; 
    private Vector3 _movement;
    private Vector2 _jumpDirection;

    public static event Action<GameObject> onGrab;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        arms = Extensions.GetChildRecursive("Arms", this.transform);
        _playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).GetComponent<Camera>();
    }
    
    private void OnEnable()
    {

    }
    
    private void OnDisable()
    {

    }

    void Update()
    {
        _movementSpeed = _isRunning ? runningSpeed : horizontalSpeed;
        _movement = (transform.forward * (!_isGrounded ? _jumpDirection.y : _direction.y) + transform.right * (!_isGrounded ? _jumpDirection.x : _direction.x)) * (_movementSpeed * Time.deltaTime);
        
        _verticalSpeed += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        _movement.y = _verticalSpeed * Time.deltaTime;

        var collisionFlags = characterController.Move(_movement);
        _isGrounded = (collisionFlags & CollisionFlags.Below) != 0;
        
        if (_isGrounded && _verticalSpeed > 0)
        {
            _verticalSpeed = 0;
        }
        
        WeaponLeaning();
        arms.transform.localRotation = Quaternion.Euler(_weaponXLean, 0, _weaponZLean);
        
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
        
            else if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if(hit.collider.tag.Contains("Interactable"))
                {
                    if (hit.distance < interactionDistance)
                    {
                        Debug.Log("Interacted with " + hit.collider.name);
                        var interactable = hit.collider.GetComponent<IInteractable>();
                        interactable?.Interact();
                    }
                }
                if (hit.collider.tag.Contains("Grabbable"))
                {
                    if (Vector3.Distance(hit.collider.transform.position, transform.position) > grabDistance) return;
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
        if(!_isGrounded || performed)
        {
            return;
        }
    
        _jumpDirection = _direction;
        _verticalSpeed = jumpSpeed;
        
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
        _verticalSpeed = 0f;
    }
}
