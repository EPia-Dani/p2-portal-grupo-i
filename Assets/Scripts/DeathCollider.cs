using System;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    private Collider _ownCollider;

    public static event Action OnPlayerDeath;

    private void OnTriggerEnter(Collider col)
    {
        if(col.tag.Contains("Player"))  OnPlayerDeath?.Invoke();
    }
}