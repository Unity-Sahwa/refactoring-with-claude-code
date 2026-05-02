using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    private Transform activeZone;
    private float elapsedTime = 0f;
    public float startScale = 0.01f;
    public float destroyDelay = 0.5f;
    public float maxScale = 0.83f;
    [HideInInspector] public float scalingTime;


    void Start()
    {
        // 바로 아래 자식 가져오기
        activeZone = transform.GetChild(0);

        if (activeZone != null)
        {
            activeZone.localScale = new Vector3(startScale, 0f, startScale);
        }
    }

    void Update()
    {
        if (activeZone != null && elapsedTime < scalingTime)
        {
            elapsedTime += Time.deltaTime;

            // Lerp를 사용하여 스케일 증가
            float currentScale = Mathf.Lerp(startScale, maxScale, elapsedTime / scalingTime);
            activeZone.localScale = new Vector3(currentScale, 0f, currentScale);
        }
        else if (elapsedTime >= scalingTime)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
