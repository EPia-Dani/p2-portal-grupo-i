using System;
using NUnit.Framework;
using UnityEngine;

public class PhysicsButton : MonoBehaviour
{
    private ConfigurableJoint _joint;
    private Rigidbody _rb;
    
    public Vector3 _startPosition = Vector3.zero;
    private bool _isPressed;
    
    public static event Action<GameObject> PhysicsButtonPressed;
    public static event Action<GameObject> PhysicsButtonReleased;
    
    
    private void Awake()
    {
        _startPosition.y = transform.position.y;
        _joint = GetComponent<ConfigurableJoint>();
        _rb = GetComponent<Rigidbody>();
    }   
    
    private void FixedUpdate()
    {
        
    }
    
    public void OnButtonPressed()
    {
        Debug.Log("Button Pressed");
        _isPressed = true;
        PhysicsButtonPressed?.Invoke(gameObject);
    }
    public void OnButtonReleased()
    {
        Debug.Log("Button Released");
        _isPressed = false;
        PhysicsButtonReleased?.Invoke(transform.parent.gameObject);
    }

    public bool IsPressed()
    {
        return _isPressed;
    }
}
