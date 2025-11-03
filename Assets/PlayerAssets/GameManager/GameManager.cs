using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static Slider _healthBar;
    private static Slider _shieldBar;
    private static TextMeshProUGUI _ammoCounter;
    private static TextMeshProUGUI _messageUI;
    private static Transform _player;
    private static Image _deathOverlay;


    void Start()
    {
        _healthBar = GetHealthBar();
        _shieldBar = GetShieldBar();
        _ammoCounter = GetAmmoCounter();
        _player = GetPlayer();
        _messageUI = GetMessageUI();
        _deathOverlay = GetDeathOverlay();
    }
    
    public static Slider GetHealthBar()
    {
        if(!_healthBar)
        {
            _healthBar = Extensions.GetChildRecursive("HealthBar", GameObject.Find("UI").transform)
                .GetComponent<Slider>();
        }
        return _healthBar;
    }

    public static Slider GetShieldBar()
    {
        if (!_shieldBar)
        {
            _shieldBar = Extensions.GetChildRecursive("ShieldBar", GameObject.Find("UI").transform)
                .GetComponent<Slider>();
        }
        return  _shieldBar;
    }

    public static TextMeshProUGUI GetAmmoCounter()
    {
        if (!_ammoCounter)
        {
            _ammoCounter = Extensions.GetChildRecursive("AmmoCount", GameObject.Find("UI").transform)
                .GetComponent<TextMeshProUGUI>();
        }
        return _ammoCounter;
    }

    public static Transform GetPlayer()
    {
        if (!_player)
        {
            _player = GameObject.Find("Player").transform;
        }
        
        return _player;
    }
    
    public static TextMeshProUGUI GetMessageUI()
    {
        if (!_messageUI)
        {
            _messageUI = Extensions.GetChildRecursive("PickupText", GameObject.Find("UI").transform)
                .GetComponent<TextMeshProUGUI>();
        }
        return _messageUI;
    }
    
    public static TextMeshProUGUI GetScoreUI()
    {
        return Extensions.GetChildRecursive("ScoreText", GameObject.Find("UI").transform)
            .GetComponent<TextMeshProUGUI>();
    }
    
    public static void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more scenes to load.");
        }
    }
    
    public static void LoadEndScene()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.Log("Scene index out of range.");
        }
    }
    
    public static void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public static Image GetDeathOverlay()
    {
        if(!_deathOverlay) return Extensions.GetChildRecursive("DeathOverlay", GameObject.Find("UI").transform)
            .GetComponent<Image>();
        return _deathOverlay;
    }
    
    
    




}
