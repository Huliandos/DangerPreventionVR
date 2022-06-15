using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    Renderer meshRenderer;
    Material baseMat;

    [SerializeField]
    Material swapMat;

    // Start is called before the first frame update
    void Start()
    {
        if (!GetComponent<Renderer>()) meshRenderer = GetComponentInChildren<Renderer>();
        else meshRenderer = GetComponent<Renderer>();

        baseMat = meshRenderer.material;
    }


    public void swapMaterial() {
        if (meshRenderer.material == baseMat) meshRenderer.material = swapMat;
        else meshRenderer.material = baseMat;
    }
}
