namespace DefaultNamespace
{
    public class LaserReceiver: ITriggerable
    {
        private bool _isTriggered = false;
        
        
        public void Trigger(bool activate)
        {
            _isTriggered = activate;
        }

        public bool IsTriggered()
        {
            return _isTriggered;
        }
    }
}