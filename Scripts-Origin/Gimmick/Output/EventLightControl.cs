using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLightControl : EventData
{

    [Header("조명 밝기")]
    [Range(0, 10)] public float targetIntensity = 1f;

    [Header("조명 밝기 변경 시간")]
    public float duration = 1f;

    private Light targetLight;

    private void Awake()
    {
        targetLight = GetComponent<Light>();
    }

    public override void Execute()
    {
        StartCoroutine(ChangeLightIntensity());
    }

    private IEnumerator ChangeLightIntensity()
    {
        float startIntensity = targetLight.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetLight.intensity = targetIntensity;
    }
}
