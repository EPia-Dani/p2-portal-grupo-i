using System;
using UnityEngine;

namespace Interactable
{
    public class EventInteractable :  MonoBehaviour, IInteractable
    {
        public static event Action<GameObject> OnInteraction;
        
        public void Interact()
        {
            Debug.Log("Interacted with " + gameObject.name);
            OnInteraction?.Invoke(this.gameObject);
        }
    }
}