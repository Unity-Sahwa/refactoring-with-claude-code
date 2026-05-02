using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            // 이동 처리
            target.transform.position = Vector3.MoveTowards(target.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 현재 위치와 이전 위치의 차이를 계산해 속도를 구함
            float speed = (target.transform.position - previousPosition).magnitude / Time.deltaTime;

            // 이전 위치 업데이트
            previousPosition = target.transform.position;

            yield return null;
        }

        // 이동이 끝났으므로 위치를 정확히 맞춤
        target.transform.position = targetPosition;
    }
}
