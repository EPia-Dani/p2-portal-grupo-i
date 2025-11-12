using System;
using UnityEngine;

public class DoorController : MonoBehaviour, ITriggerable
{
    [SerializeField] private GameObject buttonObject;
    
    private static readonly int DoorTriggered = Animator.StringToHash("DoorTriggered");

    private Animator _animator;
    private bool _isTriggered = false;
    
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    
    private void OpenDoor()
    {
        _isTriggered = true;
        _animator.SetBool(DoorTriggered, true);
    }
    
    private void CloseDoor()
    {
        _isTriggered = false;
        _animator.SetBool(DoorTriggered, false);
    }

    public void Trigger( bool activate)
    {
        if (activate)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public bool IsTriggered()
    {
        return _isTriggered;
    }
}
