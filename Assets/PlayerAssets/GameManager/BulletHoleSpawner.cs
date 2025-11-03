using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

public class BulletHoleSpawner : MonoBehaviour
{
    
    IObjectPool<DecalProjector> decalPool;
    
    public Material decalMaterial;
    
    public LayerMask decalLayer = -1;
    public Vector3 decalSize;
  

    private Camera cam;

    public float fadeDuration;


    void OnEnable()
    {
        WeaponController.BulletShot += SpawnDecal;
    }

    void OnDisable()
    {
        WeaponController.BulletShot -= SpawnDecal;
    }
    
    void Start()
    {
        cam = Camera.main;

        decalPool = new ObjectPool<DecalProjector>(
            createFunc: () =>
            {
                GameObject go = new GameObject("DecalProjector");
                DecalProjector dp = go.AddComponent<DecalProjector>();
                dp.material = decalMaterial;
                dp.fadeFactor = 1f;
                dp.fadeScale = 0.95f;
                return dp;
            },
            actionOnGet: dp => dp.gameObject.SetActive(true),
            actionOnRelease: dp => dp.gameObject.SetActive(false),
            actionOnDestroy: dp => Destroy(dp.gameObject),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 20
        );
    }
    
    private void SpawnDecal(RaycastHit hit)
    {
      DecalProjector projector = decalPool.Get();
    
      projector.transform.position = hit.point + hit.normal * 0.5f;
      
      Quaternion normalRotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
      Quaternion randomRotation  =  Quaternion.Euler(0, 0, Random.Range(0, 360f));
      projector.transform.localRotation =  normalRotation;
      
      projector.size = decalSize;
      
      StartCoroutine(FadeAndRelease(projector, fadeDuration));
    }

    IEnumerator FadeAndRelease(DecalProjector projector, float duration)
    {
        float time = 0f;
        float initialFade = projector.fadeFactor;
        

        while (time < duration)
        {
            
            time += Time.deltaTime;
            float t = time / duration;
            projector.fadeFactor = Mathf.Lerp(initialFade, 0f, t);

            yield return null;
            
            if (projector.fadeFactor < 0.1f)
            {
                projector.fadeFactor = 0f;
                projector.fadeFactor = initialFade; 
                decalPool.Release(projector);
                yield break;
            }
        }
        
        
        
        
    }
}
