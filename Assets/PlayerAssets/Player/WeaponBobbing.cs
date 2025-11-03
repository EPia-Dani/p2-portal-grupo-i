using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class WeaponBobbing : MonoBehaviour
{
    [Range(1f, 30f)]
    public float frequency = 10f;
    
    [Range(0.001f, 1f)]
    public float amount = 0.002f;
    
    [Range(10f, 100f)]
    public float smooth = 10f;

    public Vector3 startPos;
    
    public FPSCharacterController characterController;
    public Camera playerCamera;
    public Camera weaponCamera;
   
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
        characterController = GetComponent<FPSCharacterController>(); 
        
        startPos = weaponCamera.transform.position;
    }

    private void Awake()
    {
        weaponCamera = Extensions.GetChildRecursive("WeaponCamera", this.transform).GetComponent<Camera>();
        playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        
        CheckForHeadBobTrigger();
    }

    private void CheckForHeadBobTrigger()
    {
        float directionMagnitude = characterController.GetMovementDirection().magnitude;
        if (directionMagnitude > 0 && characterController.GetGrounded())
        {
            StartHeadBob();
            StopHeadBob();
        }
    }

    public void StartHeadBob()
    {
        Vector3 pos = Vector3.zero;
        
        pos.x += Mathf.Lerp(pos.x, Mathf.Sin(Time.time * (characterController.GetRunning() ? frequency * 1.5f : frequency)) * amount, smooth * Time.time);
        pos.y += Mathf.Lerp(pos.y, -Mathf.Abs(Mathf.Sin(Time.time * (characterController.GetRunning() ? frequency * 1.5f : frequency)) * amount), smooth * Time.time);
        weaponCamera.transform.localPosition = pos;
        
    }

    public void StopHeadBob()
    {
        if (weaponCamera.transform.localPosition != Vector3.zero) return;
        weaponCamera.transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, smooth * Time.time);
    }
    
}
