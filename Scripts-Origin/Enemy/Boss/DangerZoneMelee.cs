using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZoneMelee : MonoBehaviour
{
    private Transform activeZone;
    private float elapsedTime = 0f;
    public float startScale = 0.01f; // 초기 스케일 (X축 길이)
    public float destroyDelay = 0.5f; // 파괴 대기 시간
    public float maxScale = 0.83f; // 최종 X축 길이
    [HideInInspector] public float scalingTime; // 스케일 증가에 걸리는 시간

    private Material material; // 색상을 조절할 Material

    void Start()
    {
        // 바로 아래 자식을 activeZone으로 설정
        activeZone = transform.GetChild(0);

        if (activeZone != null)
        {
            // 초기 스케일 설정 (X축만 0으로 시작)
            activeZone.localScale = new Vector3(startScale, activeZone.localScale.y, activeZone.localScale.z);

            // Material 가져오기 (Renderer를 통해 접근)
            Renderer renderer = activeZone.GetComponent<Renderer>();
            if (renderer != null)
            {
                material = renderer.material;
                material.color = Color.red; // DangerZone 초기 색상
            }
        }
    }

    void Update()
    {
        if (activeZone != null && elapsedTime < scalingTime)
        {
            elapsedTime += Time.deltaTime;

            // Lerp를 사용하여 X축 스케일 증가
            float currentScaleX = Mathf.Lerp(startScale, maxScale, elapsedTime / scalingTime);
            activeZone.localScale = new Vector3(currentScaleX, activeZone.localScale.y, activeZone.localScale.z);

            // 색상을 조절 (예: 점점 더 밝게)
            if (material != null)
            {
                float progress = elapsedTime / scalingTime;
                material.color = Color.Lerp(Color.red, Color.white, progress * 0.5f); // 붉은색에서 점점 하얀색으로
            }
        }
        else if (elapsedTime >= scalingTime)
        {
            Destroy(gameObject, destroyDelay); // 지연 후 오브젝트 파괴
        }
    }
}
