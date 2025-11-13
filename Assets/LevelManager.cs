using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject orangePortal;
    [SerializeField] private GameObject bluePortal;
    [SerializeField] private GameObject[] disablingColliders;
    
    private static bool _orangeActive = false;
    private static bool _blueActive = false;
    
    private void OnEnable()
    {
        PortalDisabler.OnDisablePortals += DisableAllPortals;
    }

    private void OnDisable()
    {
        PortalDisabler.OnDisablePortals -= DisableAllPortals;
    }
    private void Update()
    {
        _orangeActive = orangePortal.activeSelf;
        _blueActive = bluePortal.activeSelf;
    }
    
    public static bool IsOrangePortalActive()
    {
        return _orangeActive;
    }
    
    public static bool IsBluePortalActive()
    {
        return _blueActive;
    }

    private void DisableAllPortals()
    {
        DisableAllCompanionCubes();
        
        if(!_orangeActive || !_blueActive)
        {
            _orangeActive = true;
            _blueActive = true;
            orangePortal.SetActive(true);
            bluePortal.SetActive(true);
        }
        else
        {
            _orangeActive = false;
            _blueActive = false;
            orangePortal.SetActive(false);
            bluePortal.SetActive(false);
        }
    }
    
    private void DisableAllCompanionCubes()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.IsValid() && obj.name.Contains("CompanionCube"))
            {
                obj.SetActive(false);
            }
        }
    }
}
