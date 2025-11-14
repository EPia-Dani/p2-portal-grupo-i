using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusManager : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image damageImage;
    [SerializeField] private float healDelay = 5f;
    [SerializeField] private float healRate = 0.3f;
    [SerializeField] private float healAmount = 10f;
    
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;
    
    private float _lastDamageTime;
    private float _lastHealTime;
    private bool _isHealing;
    private float _currentHealth;
    private float _alphaVelocity;
    private bool _isDead = false;
    
    private AudioManager _audioManager;

    public static event Action OnPlayerDeath;
    
    
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        
        Color damageColor = damageImage.color;
        Color startColor = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);
        damageImage.color = startColor;
        
        _audioManager = GetComponent<AudioManager>();
    }
    
    private void OnEnable()
    {
        DeathCollider.OnPlayerDeath += PlayerInstaKill;
    }
    private void OnDisable()
    {
        DeathCollider.OnPlayerDeath -= PlayerInstaKill;
    }
    
    private void Update()
    {
        if (_currentHealth < maxHealth && Time.time - _lastDamageTime >= healDelay && !_isDead)
        {
            
            if (!_isHealing)
            {
                _isHealing = true;
            }

            if (Time.time - _lastHealTime >= healRate && _isHealing)
            {
                if(_currentHealth < maxHealth && _currentHealth + healAmount >= maxHealth)
                {
                   _audioManager.PlaySfx(healSound, 5f);
                }
                _currentHealth += healAmount;
                _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
                _lastHealTime = Time.time;
                
                if(_currentHealth >= maxHealth) _isHealing = false;
            }
        }
        
        UpdateDamageImageAlpha();
    }
    
    public void TakeDamage(float damage)
    {
        
        _audioManager.PlaySfx(hurtSound, 5f);
        
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        _lastDamageTime = Time.time;
        _isHealing = false;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void UpdateDamageImageAlpha()
    {
        float healthPercentage = 1f- _currentHealth / maxHealth;

        Color damageColor = damageImage.color;
        float currentAlpha = damageColor.a;
        float smoothAlpha = Mathf.Lerp(currentAlpha, healthPercentage, 15f * Time.deltaTime);

        damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, smoothAlpha);
    }
    

    private void Die()
    {
        
        _audioManager.PlaySfx(deathSound, 5f);
        _isDead = true;
        OnPlayerDeath?.Invoke();
    }

    private void PlayerInstaKill()
    {
        TakeDamage(999f);
    }
}
