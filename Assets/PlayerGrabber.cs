using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    private Transform _dummyObject;
    private GameObject _currentGrabbedObject;
    private bool _isGrabbing;
    
    public static event Action<Transform, GameObject> objectGrabbed;

    void Awake()
    {
        _dummyObject = Extensions.GetChildRecursive("DummyObject", transform).transform;
        _isGrabbing = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        FPSCharacterController.onGrab += GrabObject;
    }

    void OnDisable()
    {
        FPSCharacterController.onGrab -= GrabObject;
    }

    private void GrabObject(GameObject toGrabObject)
    {
        if(toGrabObject) _currentGrabbedObject = toGrabObject;
        objectGrabbed?.Invoke(_dummyObject.transform, toGrabObject ? toGrabObject : _currentGrabbedObject);
        if(!toGrabObject) _currentGrabbedObject = null;
    }
    
}
