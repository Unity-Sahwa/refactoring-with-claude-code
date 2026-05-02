using UnityEngine.AI;
using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    [Header("인디케이터가 가리킬 타겟")]
    public Transform target;

    [Header("인디케이터 이미지 (UI)")]
    public Image indicator;

    [Header("화면 경계로부터의 거리")]
    public float edgeOffset = 10f;

    [Header("점멸 주기 설정")]
    public float blinkSpeed = 1f;

    public Camera mainCamera;

    private void Update()
    {
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position);

        if (screenPosition.z < 0)
        {
            screenPosition *= -1;
            screenPosition.z = 0;
        }

        bool isOnScreen = screenPosition.z > 0 && screenPosition.x > 0 && screenPosition.x < Screen.width
            && screenPosition.y > 0 && screenPosition.y < Screen.height;

        if (isOnScreen)
        {
            indicator.transform.position = screenPosition;
        }
        else
        {
            screenPosition.x = Mathf.Clamp(screenPosition.x, edgeOffset, Screen.width - edgeOffset);
            screenPosition.y = Mathf.Clamp(screenPosition.y, edgeOffset, Screen.height - edgeOffset);

            indicator.transform.position = screenPosition;
        }

        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color color = indicator.color;
        color.a = alpha;
        indicator.color = color;
    }
}
