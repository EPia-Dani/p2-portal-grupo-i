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


    private void Start()
    {
        _player = GetPlayer();
    }


    public static Transform GetPlayer()
    {
        if (_player == null)
        {
            _player = GameObject.Find("Player").transform;
            return _player;
        }
        else return _player;
    }



}
