using System;
using System.Collections;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    
    public static PlayerAudioManager instance;
    
    private Coroutine _currentCoroutine;
    
    private void Awake()
    {
        instance = this;
    }

    

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        
        if(_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(PlaySFXCoroutine(clip, volume));
    }
    
    private IEnumerator PlaySFXCoroutine(AudioClip clip, float volume)
    {
        
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        yield return new WaitForSeconds(clip.length);
        
        Destroy(source);
    }
}
