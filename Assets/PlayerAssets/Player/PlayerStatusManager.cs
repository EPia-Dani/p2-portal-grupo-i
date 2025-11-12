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
    
    private float _lastDamageTime;
    private float _lastHealTime;
    private bool _isHealing;
    private float _currentHealth;
    private float _alphaVelocity;

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
        if (_currentHealth < maxHealth && Time.time - _lastDamageTime >= healDelay)
        {
            
            if (!_isHealing)
            {
                _isHealing = true;
            }

            if (Time.time - _lastHealTime >= healRate && _isHealing)
            {
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
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        _lastDamageTime = Time.time;
        _isHealing = false;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    // private void UpdateDamageImageAlpha()
    // {
    //     float healthPercentage = 1f - _currentHealth / maxHealth;
    //     float targetAlpha = healthPercentage;
    //     
    //     Color damageColor = damageImage.color;
    //     float currentAlpha = damageColor.a;
    //     float smoothAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 20f);
    //
    //     damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, smoothAlpha);
    // }
    
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
        Debug.Log("Player has died.");
        OnPlayerDeath?.Invoke();
    }

    private void PlayerInstaKill()
    {
        TakeDamage(999f);
    }
}
