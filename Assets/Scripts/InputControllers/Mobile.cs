namespace InputControllers
{
    public class Mobile : InputControllerBase
    {
        protected override void OnClicked()
        {
            OnClickedEvent.Invoke();
        }
    }
}
