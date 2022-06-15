﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBloodPrefabSettings : MonoBehaviour
{
    public float AnimationSpeed = 1;
    public float GroundHeight = 0;
    [Range(0, 1)]
    public float LightIntensityMultiplier = 1;
    public bool FreezeDecalDisappearance = false;
    public _DecalRenderinMode DecalRenderinMode = _DecalRenderinMode.Floor_XZ;
    public bool ClampDecalSideSurface = false;
    public Vector3 scaling;

    public enum _DecalRenderinMode
    {
        Floor_XZ,
        AverageRayBetwenForwardAndFloor
    }
}