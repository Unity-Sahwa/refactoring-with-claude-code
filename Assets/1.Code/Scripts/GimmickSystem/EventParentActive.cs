using UnityEngine;

namespace Refactoring
{
    public class EventParentActive : EventData
    {
        public bool setActive = true;

        public override void Execute()
        {
            if (transform.parent != null)
                transform.parent.gameObject.SetActive(setActive);
        }
    }
}
