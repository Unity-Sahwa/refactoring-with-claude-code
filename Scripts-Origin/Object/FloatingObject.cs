using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public enum Direction
    {
        X, // X축 방향
        Y, // Y축 방향
        Z  // Z축 방향
    }

    [Header("움직임 방향")]
    public Direction movementDirection;

    [Header("떠다니는 높이 또는 폭 (진폭)")]
    public float height = 0.5f;

    [Header("떠다니는 속도 (주기)")]
    public float speed = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * height;

        switch (movementDirection)
        {
            case Direction.X:
                transform.position = new Vector3(startPos.x + offset, startPos.y, startPos.z);
                break;

            case Direction.Y:
                transform.position = new Vector3(startPos.x, startPos.y + offset, startPos.z);
                break;

            case Direction.Z:
                transform.position = new Vector3(startPos.x, startPos.y, startPos.z + offset);
                break;
        }
    }
}
