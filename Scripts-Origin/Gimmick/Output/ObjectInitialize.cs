using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInitialize: MonoBehaviour
{
    private Vector3 initialPositions;
    private Quaternion initialRotations;

    private void Start()
    {
        initialPositions = transform.position;
        initialRotations = transform.rotation;
    }

    public void RestoreInitialState()
    {
        transform.position = initialPositions;
        transform.rotation = initialRotations;
    }
}
