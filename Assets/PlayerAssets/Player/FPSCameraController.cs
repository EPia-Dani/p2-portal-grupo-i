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
    
    private bool _skipNextUpdate = false; // Nuevo flag
    
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
    
    private void LateUpdate()
    {
        if (_skipNextUpdate)
        {
            _skipNextUpdate = false;
            return;
        }
        
        _movementSpeed = fpsCharacterController.GetMovementSpeed();
        _direction = fpsCharacterController.GetMovementDirection();
    
        _pitch = Mathf.Clamp(_pitch + (_lookDirection.y * sensitivity * Time.deltaTime * (invertYAxis ? 1 : -1)), minPitch, maxPitch);
        _yaw += _lookDirection.x * sensitivity * Time.deltaTime;
    
        pitchController.transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
    }
    
    public void Look(Vector2 direction)
    {
        _lookDirection = direction;
    }
    
    
    private void HandleTeleport(GameObject obj, Transform fromPortal, Transform toPortal)
    {
        if (obj == gameObject)
        {
            // Obtener la rotación actual de la cámara
            Quaternion currentRotation = Quaternion.Euler(0, _yaw, 0) * Quaternion.Euler(_pitch, 0, 0);

            // Aplicar la misma transformación que en TeleportableObject
            Quaternion localRot = Quaternion.Inverse(fromPortal.rotation) * currentRotation;
            Quaternion mirror = Quaternion.Euler(0, 180, 0);
            localRot = mirror * localRot;
            Quaternion newRotation = toPortal.rotation * localRot;

            // Extraer yaw y pitch
            Vector3 euler = newRotation.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;
            if (_pitch > 180) _pitch -= 360;

            _skipNextUpdate = true;
        }
    }
}
