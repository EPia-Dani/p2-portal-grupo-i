using UnityEngine;

public class ButtonActivator : MonoBehaviour
{
    private Collider _collider;
    
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip buttonReleaseSound;
    
    private AudioManager _audioManager;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        _audioManager = GetComponent<AudioManager>();
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var button = other.GetComponent<PhysicsButton>();

        if (button && !button.IsPressed())
        {
            button.OnButtonPressed();
            _audioManager.PlaySfx(buttonSound, 3f);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var button = other.GetComponent<PhysicsButton>();

        if (button && button.IsPressed())
        {
            button.OnButtonReleased();
            _audioManager.PlaySfx(buttonReleaseSound, 0.1f);
        }
        
    }
}
