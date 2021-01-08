namespace PushbackHelper
{
    class ServicesManager
    {
        private SimConnectManager myManager;
        public ServicesManager(SimConnectManager manager)
        {
            myManager = manager;
        }
        public void ToggleJetway()
        {
            myManager.TransmitEvent(EventsEnum.KEY_TOGGLE_JETWAY, 1);
        }
        public void ToggleFuel()
        {
            myManager.TransmitEvent(EventsEnum.KEY_REQUEST_FUEL, 1);
        }
    }
}
