using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    
    [SerializeField] private float minDistanceToGrabbedObject = 4f;
    
    [SerializeField] private float maxGrabDistance = 6f;
    
    private Transform _dummyObject;
    private GameObject _currentGrabbedObject;
    
    public static event Action<Transform, GameObject> objectGrabbed;

    void Awake()
    {
        _dummyObject = Extensions.GetChildRecursive("DummyObject", transform).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentGrabbedObject != null)
        {
            Vector3 directionToPlayer = _dummyObject.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;
            
            if (distanceToPlayer < minDistanceToGrabbedObject)
            {
                _dummyObject.position = transform.position + directionToPlayer.normalized * minDistanceToGrabbedObject;
            }
        }
    }

    void OnEnable()
    {
        FPSCharacterController.onGrab += GrabObject;
        TeleportableObject.OnTeleport += OnTeleport;
    }

    void OnDisable()
    {
        FPSCharacterController.onGrab -= GrabObject;
        TeleportableObject.OnTeleport -= OnTeleport;
    }

    private void GrabObject(GameObject toGrabObject)
    {
        if(toGrabObject) _currentGrabbedObject = toGrabObject;
        objectGrabbed?.Invoke(_dummyObject.transform, toGrabObject ? toGrabObject : _currentGrabbedObject);
        if(!toGrabObject) _currentGrabbedObject = null;
    }

    private void OnTeleport(GameObject toTeleport , Transform fromPortal, Transform toPortal)
    {
        if (toTeleport == gameObject || toTeleport == _currentGrabbedObject)
        {
            GrabObject(_currentGrabbedObject);
            _currentGrabbedObject = null;
        }
        
    }
    
}
