using UnityEngine;

public class Radio : MonoBehaviour, ITriggerable
{
    [SerializeField] private AudioClip radioClip;
    [SerializeField] private GameObject radioObjective;
    
    private AudioManager audioManager;
    
    private bool _isPlaying;
    
    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();
    }

    private void OnEnable()
    {
        LaserReceiver.SecondaryObjectiveTriggered += EventTriggerRadioObjective;
    }
    
    private void OnDisable()
    {
        LaserReceiver.SecondaryObjectiveTriggered -= EventTriggerRadioObjective;
    }
    
    private void EventTriggerRadioObjective(GameObject obj, bool activated)
    {
        if (obj == radioObjective)
        {
            Trigger(activated);
        }
    }
    
    
    public void Trigger(bool activate)
    {
        if (!_isPlaying && activate)
        {
            audioManager.PlaySfxLoop(radioClip, 5f);
            _isPlaying = true;
        }
    }

    public bool IsTriggered()
    {
        return _isPlaying;
    }
}
