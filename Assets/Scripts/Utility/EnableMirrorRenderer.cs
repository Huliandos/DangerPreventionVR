using HTC.UnityPlugin.StereoRendering;
using System.Collections;
using UnityEngine;
using VRTK;

public class EnableMirrorRenderer : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(FindHMD());
    }

    IEnumerator FindHMD()
    {
        while (VRTK_DeviceFinder.HeadsetCamera() == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        GameObject go = gameObject.GetComponentInChildren<StereoRenderer>(true).gameObject;
        if (!go.activeSelf) go.SetActive(true);
    }
}
