using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PatchThePatchSDZ : MonoBehaviour
{
    private SnapDropZonePatch snapZonePatch;

    private bool forceSnappingPositionRotation;
    private int frameCounter = 3;

    private void Awake()
    {
        snapZonePatch = GetComponent<SnapDropZonePatch>();

        snapZonePatch.onObjectSuccesfullySnapped += () => StartCoroutine(hackPositionCoroutine());
        snapZonePatch.onObjectSuccesfullyUnsnapped += () => forceSnappingPositionRotation = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if (forceSnappingPositionRotation && snapZonePatch.SnappedObject != null)
        //{
        //    forceSnappingPositionRotation = false;
            
        //}
    }

    private IEnumerator hackPositionCoroutine()
    {
        forceSnappingPositionRotation = true;
        while(frameCounter > 0)
        {
            snapZonePatch.SnappedObject.transform.localPosition = Vector3.zero;
            snapZonePatch.SnappedObject.transform.eulerAngles = Vector3.zero;

            frameCounter--;
            yield return null;
        }

        frameCounter = 3;
    }
}
