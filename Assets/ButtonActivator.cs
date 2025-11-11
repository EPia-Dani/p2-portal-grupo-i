using UnityEngine;

public class ButtonActivator : MonoBehaviour
{
    private Collider _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var button = other.GetComponent<PhysicsButton>();

        if (button && !button.IsPressed()) button.OnButtonPressed();
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name);
        var button = other.GetComponent<PhysicsButton>();

        if (button && button.IsPressed()) button.OnButtonReleased();
    }
}
