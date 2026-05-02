using System.Collections;
using UnityEngine;

public class EventPushAway : MonoBehaviour
{
    [Header("밀어내는 힘")]
    public float pushForce = 100f;

    [Header("바람 지속 시간")]
    public float windTime = 5f;

    [Header("쿨타임")]
    public float pushCooldown = 10f;

    [Header("바람이 시작될 지점")]
    public Transform windSource;

    [Header("바람 범위")]
    public BoxCollider boxForWindRange;

    private Coroutine windCoroutine;

    private void OnEnable()
    {
        if (windSource == null)
        {
            Debug.LogWarning("windSource가 지정되지 않았습니다!");
            return;
        }
        if (boxForWindRange == null)
        {
            Debug.LogWarning("BoxCollider가 지정되지 않았습니다!");
            return;
        }

        // 코루틴 시작
        windCoroutine = StartCoroutine(WindCycleRoutine());
    }

    private void OnDisable()
    {
        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }
    }

    private IEnumerator WindCycleRoutine()
    {
        while (true)
        {
            float endTime = Time.time + windTime;
            while (Time.time < endTime)
            {
                // 콜라이더 로컬 좌표계 정보
                Vector3 localCenter = boxForWindRange.center;
                Vector3 localSize = boxForWindRange.size;

                // 월드로 변환
                Vector3 worldCenter = boxForWindRange.transform.TransformPoint(localCenter);
                // 스케일을 곱해, 실제 크기로 맞추기
                Vector3 scaledSize = Vector3.Scale(localSize, boxForWindRange.transform.lossyScale);
                // 가로 세로 높이의 절반 길이를 담은 벡터
                Vector3 halfExtents = scaledSize * 0.5f;

                // 콜라이더 회전
                Quaternion orientation = boxForWindRange.transform.rotation;

                Collider[] colliders = Physics.OverlapBox(worldCenter, halfExtents, orientation);
                foreach (Collider otherCollider in colliders)
                {
                    Rigidbody otherRigidbody = otherCollider.attachedRigidbody;
                    if (otherRigidbody != null)
                    {
                        // 바람 방향
                        Vector3 pushDir = windSource.forward.normalized;

                        // AddForce로 지속적으로 밀어내기
                        otherRigidbody.AddForce(pushDir * pushForce * Time.deltaTime, ForceMode.Force);
                    }
                }
                yield return null;
            }

            yield return new WaitForSeconds(pushCooldown);
        }
    }

    private void OnDrawGizmos()
    {
        // 바람 방향 표시
        if (windSource != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(windSource.position, windSource.forward * 3f);
        }
    }
}