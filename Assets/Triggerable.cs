using UnityEngine;


public interface ITriggerable
{ 
    public void Trigger(bool activate);
    
    public bool IsTriggered();
}
