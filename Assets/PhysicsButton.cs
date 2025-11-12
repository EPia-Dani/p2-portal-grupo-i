using System;
using NUnit.Framework;
using UnityEngine;

public class PhysicsButton : MonoBehaviour
{
    [SerializeField] private GameObject triggerObj;
    
    private ITriggerable _trigger;
    
    private ConfigurableJoint _joint;
    private Rigidbody _rb;
    
    public Vector3 _startPosition = Vector3.zero;
    private bool _isPressed;
    
    
    private void Awake()
    {
        _startPosition.y = transform.position.y;
        _joint = GetComponent<ConfigurableJoint>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if(triggerObj.GetComponent<ITriggerable>() != null) 
            _trigger = triggerObj.GetComponent<ITriggerable>();
    }
    
    public void OnButtonPressed()
    {
        _isPressed = true;
        _trigger.Trigger(true);
    }
    public void OnButtonReleased()
    {

        _isPressed = false;
        _trigger.Trigger(false);
    }

    public bool IsPressed()
    {
        return _isPressed;
    }
}
