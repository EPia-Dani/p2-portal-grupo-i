using UnityEngine;

public class LaserReceiver: MonoBehaviour, ITriggerable
{
    [SerializeField] private GameObject triggerObj;
    [SerializeField] private AudioClip activateSound;
    
    private AudioManager _audioManager;
    private ITriggerable _trigger;
    private bool _isTriggered = false;
    private MaterialPropertyChanger _matChanger;
    
    private void Start()
    {
        _audioManager = GetComponent<AudioManager>();
        if(triggerObj.GetComponent<ITriggerable>() != null) 
            _trigger = triggerObj.GetComponent<ITriggerable>();
    }
    
    private void Awake()
    {
        _matChanger = GetComponent<MaterialPropertyChanger>();
    }
    
    public void Trigger(bool activate)
    {
        if (_isTriggered == activate) return;

        if (activate)
        {
            _matChanger.ChangeEmissionWithPropertyBlock(Color.green);
            _audioManager.PlaySfx(activateSound, 1f);
        }
        else _matChanger.ChangeEmissionWithPropertyBlock(Color.white);
        _isTriggered = activate;
        _trigger?.Trigger(activate);
    }

    public bool IsTriggered()
    { 
        return _isTriggered;
    } 
}
