using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject orangePortal;
    [SerializeField] private GameObject bluePortal;
    [SerializeField] private GameObject player;

    private static bool _orangeActive = false;
    private static bool _blueActive = false;
    private static Vector3 _lastCheckpointPosition;
    private static Quaternion _lastCheckpointRotation;
    private static bool _hasCheckpoint = false;

    private static LevelManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeReferences();
    }

    private void OnEnable()
    {
        PortalDisabler.OnDisablePortals += DisableAllPortals;
        CheckpointCollider.OnCheckpointReached += SaveCheckpoint;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        PortalDisabler.OnDisablePortals -= DisableAllPortals;
        CheckpointCollider.OnCheckpointReached -= SaveCheckpoint;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        _orangeActive = orangePortal != null && orangePortal.activeSelf;
        _blueActive = bluePortal != null && bluePortal.activeSelf;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeReferences();

        player = GameObject.FindGameObjectWithTag("Player");

        if (_hasCheckpoint && player != null)
        {
            var rb = player.GetComponent<Rigidbody>();
            var cc = player.GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
                player.transform.SetPositionAndRotation(_lastCheckpointPosition, _lastCheckpointRotation);
                cc.enabled = true;
            }
            else if (rb != null)
            {
                rb.position = _lastCheckpointPosition;
                rb.rotation = _lastCheckpointRotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
            }
            else
            {
                player.transform.SetPositionAndRotation(_lastCheckpointPosition, _lastCheckpointRotation);
            }
        }
    }

    private void InitializeReferences()
    {
        if (orangePortal == null)
            orangePortal = GameObject.Find("PortalOrange");
        if (bluePortal == null)
            bluePortal = GameObject.Find("PortalBlue");
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    private void SaveCheckpoint(Vector3 position, Quaternion rotation)
    {
        _lastCheckpointPosition = position;
        _lastCheckpointRotation = rotation;
        _hasCheckpoint = true;
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsOrangePortalActive()
    {
        return _orangeActive;
    }

    public bool IsBluePortalActive()
    {
        return _blueActive;
    }

    private void DisableAllPortals()
    {
        DisableAllCompanionCubes();

        if (!_orangeActive || !_blueActive)
        {
            _orangeActive = true;
            _blueActive = true;
            orangePortal?.SetActive(true);
            bluePortal?.SetActive(true);
        }
        else
        {
            _orangeActive = false;
            _blueActive = false;
            orangePortal?.SetActive(false);
            bluePortal?.SetActive(false);
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