using System.Collections;
using UnityEngine;
using VRTK;

public class PlayAreaWrapper : MonoBehaviour
{
    [SerializeField] GameObject playArea;

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

        Debug.Log($"Found {VRTK_DeviceFinder.HeadsetCamera()}, activating {playArea}");
        if (!playArea.activeSelf) playArea.SetActive(true);
    }
}
