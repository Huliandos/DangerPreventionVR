using System;
using System.Collections;
using UnityEngine;
using VRTK;
using Utility;

namespace Challenges
{
    [Serializable]
    [RequireComponent(typeof(VRTK_SnapDropZone), typeof(SnapDropZonePatch))]
    public class CheckableSnapZone : MonoBehaviour
    {
        #region public fields

        public GameObject measurementHightlightOBject;

        public bool AcceptingCalls
        {
            get => acceptingCalls;
            set
            {
                acceptingCalls = value;
                if (preventDisableHighlighting == null) return;
                if (value == false) preventDisableHighlighting.NotifyDisable = null;
                if (value) preventDisableHighlighting.NotifyDisable = () => checkForReEnable = true;
            }
        }

        #endregion

        #region private fields

        private SnapDropZonePatch dropZone;
        private VRTK_SnapDropZone vrtk_dropZone;
        private GameObject currentlySnappedObject;
        private Bounds snapZoneBounds;
        private bool acceptingCalls = true;
        private PreventDisable preventDisableHighlighting;
        private bool checkForReEnable;
        
        #endregion

        #region Debugging

        private Bounds snapZoneBoundaries;
        private Bounds objectBounds;
        private bool rdyToDraw;

        #endregion


        private void Awake()
        {
            dropZone = GetComponent<SnapDropZonePatch>();
            vrtk_dropZone = GetComponent<VRTK_SnapDropZone>();
            MeshRenderer snapZoneHightlightRenderer = measurementHightlightOBject.GetComponent<MeshRenderer>();
            snapZoneBounds = snapZoneHightlightRenderer.bounds;

            preventDisableHighlighting = measurementHightlightOBject.AddComponent<PreventDisable>();
            
            preventDisableHighlighting.NotifyDisable += () => checkForReEnable = true;
            dropZone.onObjectSuccesfullySnapped += OnObjectHandedIn;
            vrtk_dropZone.ObjectExitedSnapDropZone += OnObjectExitedDropZone;

            AcceptingCalls = true;
        }

        private void LateUpdate()
        {
            if (currentlySnappedObject != null)
                currentlySnappedObject.transform.rotation = dropZone.OriginalObjectYRotation;

            if (checkForReEnable)
            {
                checkForReEnable = false;
                if (!AcceptingCalls) return;
                StartCoroutine(preventDisableHighlighting.EnableObject());
            }
        }

        private void OnDisable()
        {
            AcceptingCalls = false;
            Destroy(measurementHightlightOBject.GetComponent<PreventDisable>());
        }

        private void OnDestroy()
        {
            dropZone.onObjectSuccesfullySnapped -= OnObjectHandedIn;
            vrtk_dropZone.ObjectExitedSnapDropZone -= OnObjectExitedDropZone;
        }

        private void OnObjectHandedIn()
        {
            currentlySnappedObject = dropZone.SnappedObject;
            currentlySnappedObject.transform.SetParentUnscaled(transform);
        }
        
        private void OnObjectExitedDropZone(object sender, SnapDropZoneEventArgs e)
        {
            if (currentlySnappedObject == null || e.snappedObject != currentlySnappedObject) return;
            currentlySnappedObject = null;
        }

        private void OnObjectRemoved(object sender, SnapDropZoneEventArgs dropZoneEventArgs)
        {
            //currentlySnappedObject = null;
            Debug.Log("unsnap");
        }

        /// <summary>
        /// checks measurements of snapdropzone and attached object
        /// </summary>
        /// <param name="callback">Action that is performed after evaluating the size difference</param>
        public void CheckMeasurements(Action<ChallengeDataResult> callback = default, Action<GameObject, ChallengeDataResult> callback2 = default)
        {
            if (currentlySnappedObject == null)
            {
                callback?.Invoke(new ChallengeDataResult());
                callback2?.Invoke(measurementHightlightOBject, new ChallengeDataResult());
                return;
            }

            MeshRenderer snappedObjectRenderer = currentlySnappedObject.GetComponent<MeshRenderer>();
            Bounds snappedObjectBounds = snappedObjectRenderer.bounds;

            /*//debugging for drawing encapsulation
            objectBounds = snappedObjectBounds;
            snapZoneBoundaries = measurementHightlightOBject.GetComponent<MeshRenderer>().bounds;
            rdyToDraw = true;*/

            //encapsulate snapzone with object
            Bounds newBounds = new Bounds(snapZoneBounds.center, snapZoneBounds.size);
            newBounds.Encapsulate(snappedObjectBounds);

            //get volume of encapsulation
            Vector3 newBoundsSize = newBounds.size;
            float volumeEncapsulation = newBoundsSize.x * newBoundsSize.y * newBoundsSize.z;

            //check volumes of object and snapzone
            float volumeOriginalZone = MeasurementChecker.CheckMeasurements(measurementHightlightOBject);
            float volumeSnappedObject = MeasurementChecker.CheckMeasurements(currentlySnappedObject);

            //get ratios how big role object and snapzone are playing in the encapsulation
            float dropZoneEncapsulationRatio = volumeOriginalZone / volumeEncapsulation;
            float snappedObjectEncapsulationRatio = volumeSnappedObject / volumeEncapsulation;

            //calculate criterias how good the object is fitting in relation
            float ratioDifference = Mathf.Abs(dropZoneEncapsulationRatio - snappedObjectEncapsulationRatio);
            float encapsulationIncreasePercentage = volumeEncapsulation / volumeOriginalZone;

            ChallengeDataResult challengeDataResult = new ChallengeDataResult(ratioDifference, encapsulationIncreasePercentage);

            Debug.Log("Variation of Ratio as Difference: " + ratioDifference + " Increase of necessary encapsulation: " + encapsulationIncreasePercentage);

            callback?.Invoke(challengeDataResult);
            challengeDataResult.ReferencedGameObject = currentlySnappedObject;
            callback2?.Invoke(measurementHightlightOBject, challengeDataResult);
        }

        /// <summary>
        /// only for debugging purposes
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!rdyToDraw) return;

            Gizmos.color = Color.green;

            Bounds newBounds = new Bounds(snapZoneBoundaries.center, snapZoneBoundaries.size);
            newBounds.Encapsulate(objectBounds);

            Gizmos.DrawCube(newBounds.center, newBounds.size);

            var transform = currentlySnappedObject.transform;

            foreach (Vector3 sharedMeshVertex in currentlySnappedObject.GetComponent<MeshFilter>().sharedMesh.vertices)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(sharedMeshVertex), 0.001f);
            }
        }
    }
}