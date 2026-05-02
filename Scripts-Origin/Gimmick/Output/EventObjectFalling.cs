using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectFalling : EventData
{
    [Header("떨어지는 시간(이후 바로 파괴)")]
    public float destroyTime = 3f;

    [Header("흔들림 지속 시간")]
    public float shakeDuration = 2f;

    [Header("흔들림 세기")]
    public float shakeMagnitude = 0.1f;

    [Header("흔들림 속도")]
    public float shakeSpeed = 20f;

    private float gravity = -9.81f;
    private float verticalSpeed = 0.0f;

    public override void Execute()
    {
        StartCoroutine(ShakeAndFall());
    }

    private IEnumerator ShakeAndFall()
    {
        var elapsedTime = 0.0f;
        var originalPosition = transform.position;

        while (elapsedTime < shakeDuration)
        {
            float shake = Mathf.Sin(elapsedTime * shakeSpeed) * shakeMagnitude;

            transform.position = new Vector3(originalPosition.x + shake, originalPosition.y, originalPosition.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        elapsedTime = 0.0f;
        while (elapsedTime < destroyTime)
        {
            verticalSpeed += gravity * Time.deltaTime;

            transform.position += new Vector3(0, verticalSpeed * Time.deltaTime, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}