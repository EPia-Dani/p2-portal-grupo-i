using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointCollider : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    
    public static event Action<Vector3, Quaternion> OnCheckpointReached;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        spawnPoint = Extensions.GetChildRecursive("SpawnPoint", transform).transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRot = Quaternion.Euler(spawnPoint.eulerAngles.x, spawnPoint.eulerAngles.y, spawnPoint.eulerAngles.z);

            OnCheckpointReached?.Invoke(spawnPosition, spawnRot);
            
            gameObject.SetActive(false);
        }
    }
    
}