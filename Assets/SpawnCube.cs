using UnityEngine;
using System.Collections.Generic;

public class SpawnCube : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int maxPoolSize = 10;
    [SerializeField] private Transform spawnPoint;
    
    [SerializeField] private GameObject buttonObject;
    
    private readonly Queue<GameObject> _cubePool = new Queue<GameObject>();
    
    

    private void Start()
    {
        spawnPoint = Extensions.GetChildRecursive("SpawnPoint", transform).transform;
    }

    private void OnEnable()
    {
        PhysicsButton.PhysicsButtonPressed += SpawnPrefab;
    }
    
    private void OnDisable()
    {
        PhysicsButton.PhysicsButtonPressed -= SpawnPrefab;
    }
    
    private void SpawnPrefab(GameObject obj)
    {
        if(obj.transform.parent.gameObject != buttonObject) return;
        
        GameObject cube;
        
        if (_cubePool.Count < maxPoolSize)
        {
            cube = Instantiate(cubePrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            cube = _cubePool.Dequeue();
            cube.transform.position = spawnPoint.position;
            cube.transform.rotation = spawnPoint.rotation;
            cube.SetActive(true);
        }

        _cubePool.Enqueue(cube);
    }
}
