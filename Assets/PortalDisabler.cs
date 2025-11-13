using System;
using Unity.VisualScripting;
using UnityEngine;

public class PortalDisabler : MonoBehaviour
{
    public static event Action OnDisablePortals;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) OnDisablePortals?.Invoke();
    }
}
