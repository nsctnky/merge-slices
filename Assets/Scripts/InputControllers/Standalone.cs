using UnityEngine;

namespace InputControllers
{
    public class Standalone : InputControllerBase
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnClicked();
            }
        }

        protected override void OnClicked()
        {
            OnClickedEvent.Invoke();
        }
    }
}
