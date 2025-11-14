using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager instance;
    
    private Coroutine _currentCoroutine;
    
    private void Awake()
    {
        instance = this;
    }

    

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        Debug.Log("Playing SFX: " + clip.name);
        if(_currentCoroutine != null) StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(PlaySfxCoroutine(clip, volume));
    }
    
    public void PlaySfxLoop(AudioClip clip, float volume = 1f)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }
    
    private IEnumerator PlaySfxCoroutine(AudioClip clip, float volume)
    {
        
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        yield return new WaitForSeconds(clip.length);
        
        Destroy(source);
    }
}
