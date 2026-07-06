using UnityEngine;

namespace Refactoring
{
    public class EventObjectActiveTrue : EventData
    {
        public Vector3 relativeSpawnPosition;

        public override void Execute()
        {
            transform.position += relativeSpawnPosition;
            gameObject.SetActive(true);
        }
    }
}
