using System;
using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    //FOV
    public static float MinFov;
    public static float MaxFov;
    public float fov = 90;
    public float fovSmoothSpeed = 0.3f;
    public float fovSpeedBias = 0.5f;
    
    
    //CAMERA CONTROL
    public float sensitivity = 1f;
    public bool invertYAxis;
    public float minPitch = -90f;
    public float maxPitch = 90f;
    
    //CAMERAS
    public Camera playerCamera;
    public Camera weaponCamera;
    
    public GameObject pitchController;
    public FPSCharacterController fpsCharacterController;
    
    private float _yaw;
    private float _pitch;
    private float _fov;

    private Vector3 _direction;
    private float _movementSpeed;

    private Vector2 _lookDirection;
    
   
    
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        MinFov = fov - 20;
        MaxFov = fov + 20;
        
        fpsCharacterController = GetComponent<FPSCharacterController>();
        
        
    }

    private void Awake()
    {
        playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).GetComponent<Camera>();
        playerCamera.fieldOfView = fov;
        
        weaponCamera = Extensions.GetChildRecursive("WeaponCamera", this.transform).GetComponent<Camera>();
        weaponCamera.fieldOfView = fov;
        
        pitchController = Extensions.GetChildRecursive("PitchController", this.transform).gameObject;
    }
    
    private void OnEnable()
    {
        TeleportableObject.OnTeleport += HandleTeleport;
    }

    private void OnDisable()
    {
        TeleportableObject.OnTeleport -= HandleTeleport;
    }
    
    private void LateUpdate() // Cambiar Update por LateUpdate
    {
        _movementSpeed = fpsCharacterController.GetMovementSpeed();
        _direction = fpsCharacterController.GetMovementDirection();
    
        _fov = CalculateFov(_movementSpeed, fpsCharacterController.horizontalSpeed, fov, MaxFov, fovSpeedBias, _direction);
        _yaw = transform.eulerAngles.y;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, _fov, fovSmoothSpeed);
        weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, _fov, fovSmoothSpeed);
    
        _pitch = Mathf.Clamp(_pitch + (_lookDirection.y * sensitivity * Time.deltaTime * (invertYAxis ? 1 : -1)), minPitch, maxPitch);
        _yaw += _lookDirection.x * sensitivity * Time.deltaTime;
    
        pitchController.transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
    }
    
    public void Look(Vector2 direction)
    {
        _lookDirection = direction;
    }


    private static float CalculateFov(float _movementSpeed, float horizontalSpeed, float baseFov, float maxFov, float fovSpeedBias, Vector2 _direction)
    {

        // Evitamos divisiones por cero y garantizamos que movementSpeed >= horizontalSpeed
        float speedRatio = Mathf.Max(_movementSpeed / horizontalSpeed, 1f);

        // Función logarítmica normalizada
        // log(1) = 0 → en horizontalSpeed tenemos baseFov
        float logValue = Mathf.Log(speedRatio);

        // Ajustamos con un factor de escala para que no suba demasiado rápido
        float scale = (maxFov - baseFov) / Mathf.Log((maxFov / horizontalSpeed));

        // Calculamos el FOV con base logarítmica
        float fov = baseFov + (logValue * scale) * _direction.magnitude * fovSpeedBias;

        // Clamp para que no se pase de los límites
        return Mathf.Clamp(fov, MinFov, maxFov);
    }
    
    private void HandleTeleport(GameObject obj)
    {
        if (obj == gameObject)
        {
            // Sincronizar yaw y pitch con la rotación actual del transform
            _yaw = transform.eulerAngles.y;
            _pitch = pitchController.transform.localEulerAngles.x;
            if (_pitch > 180) _pitch -= 360;
        }
    }
    
}
