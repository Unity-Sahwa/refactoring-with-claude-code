using System.Collections;
using UnityEngine;

namespace Refactoring 
{
    public class EventObjectMove : EventData
    {
        [Header("이동할 위치")]
        public Vector3 relativePosition;

        [Header("이동 속도")]
        [Range(0f, 1000f)] public float moveSpeed = 1f;

        public Vector3 previousPosition;

        private void Start()
        {
            previousPosition = transform.position;
        }

        public override void Execute()
        {
            Vector3 targetPosition = transform.position + relativePosition;
            StartCoroutine(MoveToPosition(gameObject, targetPosition));
        }

        private IEnumerator MoveToPosition(GameObject target, Vector3 targetPosition)
        {
            while (Vector3.Distance(target.transform.position, targetPosition) > 0.01f)
            {
                Vector3 direction = (targetPosition - target.transform.position).normalized;
                target.transform.position = Vector3.MoveTowards(target.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                float speed = (target.transform.position - previousPosition).magnitude / Time.deltaTime;
                previousPosition = target.transform.position;

                yield return null;
            }
            
            target.transform.position = targetPosition;
        }
    }
}




