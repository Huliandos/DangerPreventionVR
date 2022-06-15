using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootObject : MonoBehaviour
{
    [SerializeField]
    GameObject root;

    [SerializeField]
    GameObject connectedMeshRenderer;

    public GameObject getRoot()
    {
        return root;
    }
    public GameObject getMeshRenderer()
    {
        return connectedMeshRenderer;
    }
}