using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideRendering : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

}
