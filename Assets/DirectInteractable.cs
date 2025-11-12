using System;
using Interactable;
using TMPro;
using UnityEngine;

public class DirectInteractable : MonoBehaviour, IInteractable
{
    private static readonly int Activate = Animator.StringToHash("Activate");
    
    [SerializeField] private GameObject triggerObj;
    private Animator _animator;
    private ITriggerable _trigger;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        
        if(triggerObj.GetComponent<ITriggerable>() != null) 
            _trigger = triggerObj.GetComponent<ITriggerable>();
    }

    public void Interact()
    {
        
        _trigger.Trigger(true);
        _animator.SetTrigger(Activate);
    }
}
