using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    
    public FPSCharacterController charController;
    public FPSCameraController camController;
    public WeaponController weaponController;

    private bool _playerDead;

    private void Start()
    {
        _playerDead = false;
    }

    private void Awake()
    {
        charController = gameObject.GetComponent<FPSCharacterController>();
        camController = gameObject.GetComponent<FPSCameraController>();
        weaponController = gameObject.GetComponent<WeaponController>();
    }
    
    private void Update()
    {
        
    } 
    
    private void OnEnable()
    {

    }
    
    private void OnDisable()
    {
      
    }
    
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if(!_playerDead)  charController.Move(context.ReadValue<Vector2>());
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        if(!_playerDead) camController.Look(context.ReadValue<Vector2>());
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if(!_playerDead)  charController.Jump(context.performed);
    }
    
    public void OnSprint(InputAction.CallbackContext context)
    {
        if(!_playerDead) charController.Sprint(context.performed);
    }
    
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(!_playerDead) charController.Crouch(context.performed);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(!_playerDead) weaponController.Fire(context.performed);
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(!_playerDead) weaponController.Reload(context.performed);
    }
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        if(!_playerDead) charController.HandleInteraction(context.performed);
    }
    
    private void DisableInputs()
    {
        _playerDead = true;
    }
}
