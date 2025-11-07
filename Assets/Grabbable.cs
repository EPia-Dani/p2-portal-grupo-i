using System;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float grabDistance = 10f;
    
    private Rigidbody _rb;
    private Transform _dummyObject;
    private bool _isGrabbed;
    private float _originalDrag;
    private float _originalAngularDrag;
    private bool _wasKinematic;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (_rb == null)
        {
            Debug.LogError("Grabbable object needs a Rigidbody component!");
        }
    }

    void Start()
    {
        if (_isGrabbed)
        {
            
        }
    }

    void OnEnable()
    {
        PlayerGrabber.objectGrabbed += ObjectGrabbed;
    }
    
    void OnDisable()
    {
        PlayerGrabber.objectGrabbed -= ObjectGrabbed;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isGrabbed)
        {
            Vector3 targetPosition = _dummyObject.position;
            Vector3 direction = targetPosition - transform.position;
            
            _rb.linearVelocity = direction * moveSpeed;
            
            Quaternion targetRotation = _dummyObject.rotation;
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }
    
    private void ObjectGrabbed(Transform dummy, GameObject grabbedObject)
    {
        if(grabbedObject != gameObject) return;
        
        if (_isGrabbed)
        {
            OnRelease();
            return;
        }
        
        _isGrabbed = true;
        _dummyObject = dummy;
        
        _originalDrag = _rb.linearDamping;
        _originalAngularDrag = _rb.angularDamping;
        _wasKinematic = _rb.isKinematic;
        
        _rb.linearDamping = 10f;
        _rb.angularDamping = 5f;
        _rb.useGravity = false;
        
        _dummyObject.position = transform.position;
        _dummyObject.rotation = transform.rotation;
        
        Debug.Log("Object Grabbed: " + gameObject.name);
    }
    
    private void OnRelease()
    {
        _isGrabbed = false;
        _dummyObject = null;
        
        _rb.linearDamping = _originalDrag;
        _rb.angularDamping = _originalAngularDrag;
        _rb.isKinematic = _wasKinematic;
        _rb.useGravity = true;
        
        Debug.Log("Object Released: " + gameObject.name);
    }
}
