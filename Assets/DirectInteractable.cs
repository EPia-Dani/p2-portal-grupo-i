using System;
using Interactable;
using UnityEngine;

public class DirectInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject triggerObj;
    private ITriggerable _trigger;


    private void Start()
    {
        if(triggerObj.GetComponent<ITriggerable>() != null) 
            _trigger = triggerObj.GetComponent<ITriggerable>();
    }

    public void Interact()
    {
        
        _trigger.Trigger(true);
    }
}
