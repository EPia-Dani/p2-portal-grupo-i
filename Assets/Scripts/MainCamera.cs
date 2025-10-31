using UnityEngine;

public class MainCamera : MonoBehaviour
{

    PortalView[] portals;

    void Awake()
    {
        portals = FindObjectsOfType<PortalView>();
        Debug.Log("Found " + portals.Length + " portals in the scene.");
    }

    void OnPreCull()
    {

        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].SetupRenderTexture();
        }

        for (int i = 0; i < portals.Length; i++)
        {
            //portals[i].PostPortalRender();
        }

    }

}