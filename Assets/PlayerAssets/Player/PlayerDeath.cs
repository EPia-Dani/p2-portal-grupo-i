using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    
    //CAMERAS
    private Camera _playerCamera;
    private Camera _weaponCamera;
    
    
    [Header("Death effect")]
    private Image _deathOverlay; // Assign a fullscreen Image (red) in the inspector
    [SerializeField] private float rollAngle = 60f;
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float overlayMaxAlpha = 0.6f;
    [SerializeField] private float overlayHold = 0.2f;
    [SerializeField] private AudioClip playerDeathAudioClip;

    
    private void OnEnable()
    {
    }
    
    private void OnDisable()
    {
    }
    
    void Start()
    {
        _playerCamera = Extensions.GetChildRecursive("PlayerCamera", this.transform).GetComponent<Camera>();
        _weaponCamera = Extensions.GetChildRecursive("WeaponCamera", this.transform).GetComponent<Camera>();
        
        if (_deathOverlay != null)
            _deathOverlay.color = new Color(1f, 0f, 0f, 0f);
    }
    
    void Update()
    {
        
    }

    private void OnPlayerDeath()
    {
        if (this.isActiveAndEnabled)
            StartCoroutine(DeathSequence());
    }
    
    private IEnumerator DeathSequence()
    {
        if (_playerCamera == null)
            yield break;
    
        AudioManager.instance.PlaySfx(playerDeathAudioClip, 1f);
        Transform camT = _playerCamera.transform;
        Quaternion startRot = camT.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, rollAngle);

        // Roll + overlay fade in
        float t = 0f;
        Color overlayStart = new Color(1f, 0f, 0f, 0f);
        Color overlayTarget = new Color(1f, 0f, 0f, overlayMaxAlpha);

        while (t < rollDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / rollDuration);
            camT.localRotation = Quaternion.Slerp(startRot, targetRot, p);

            if (_deathOverlay != null)
                _deathOverlay.color = Color.Lerp(overlayStart, overlayTarget, p);

            yield return null;
        }

        camT.localRotation = targetRot;

        if (_deathOverlay != null)
            _deathOverlay.color = overlayTarget;

        // Hold overlay
        yield return new WaitForSeconds(overlayHold);

        //GameManager.ReloadCurrentScene(); //handled by GameOverUI now
    }
}
