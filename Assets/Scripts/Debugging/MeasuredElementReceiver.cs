using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Challenges;
using JetBrains.Annotations;
using UnityEngine;

namespace Debugging
{
    public class MeasuredElementReceiver : MonoBehaviour
    {
        [SerializeField] private Transform spawnableAnchor;
        [SerializeField] private Material overrideMaterial;
        
        [SerializeField] private LineRenderer horizontalTapeLine;
        [SerializeField] private LineRenderer verticalTapeLine;
        
        public delegate void EvaluateManually(float finalResult);
        public static EvaluateManually manualEvaluation = null;
        
        private GameObject currentSnapZone;
        private GameObject currentSnappedObject;

        private Dictionary<GameObject, ChallengeDataResult>.Enumerator enumerator;
        private bool endIteration;
        
        private void Awake()
        {
            ChallengeManager.manualReviewDelegate += OnManualReview;
            manualEvaluation = IterateEpoch;
        }

        private void IterateEpoch(float result)
        {
            if (endIteration) return;
            
            ScoreDisplay.onTransmitSingleResult.Invoke(result);
            SpawnTransmittedObjects();
        }

        private void OnManualReview([ItemCanBeNull] Dictionary<GameObject, ChallengeDataResult> mappedObjects)
        {
            enumerator = mappedObjects.GetEnumerator();
            endIteration = false;
            SpawnTransmittedObjects();
        }

        private void SpawnTransmittedObjects()
        {
            if (currentSnapZone != null) Destroy(currentSnapZone);
            if (currentSnappedObject != null) Destroy(currentSnappedObject);
            
            if (enumerator.MoveNext() && enumerator.Current.Value != null && enumerator.Current.Value != null)
                SpawnNextEpoch(enumerator.Current.Key, enumerator.Current.Value.ReferencedGameObject);
            else
            {
                ScoreDisplay.onEvaluateResultManually.Invoke();
                endIteration = true;
            }
        }

        private void SpawnNextEpoch(GameObject snapZone, GameObject mappedObject)
        {
            if (snapZone != null)
            {
                currentSnapZone = Instantiate(snapZone, spawnableAnchor);
                PlaceObjectOnPlane(currentSnapZone);
            }

            if (mappedObject != null)
            {
                currentSnappedObject = Instantiate(mappedObject, spawnableAnchor);
                PlaceObjectOnPlane(currentSnappedObject, overrideMaterial);
            }
        }

        /// <summary>
        /// Method to locate the spawned transmitted object directly in place for ortographic camera view
        /// adjustes opacity and replaces material if necessary
        /// </summary>
        /// <param name="placeableObject">to be placed object</param>
        /// <param name="newMaterial">exchanging material</param>
        private void PlaceObjectOnPlane(GameObject placeableObject, Material newMaterial = default)
        {
            placeableObject.transform.localPosition = Vector3.zero;
            placeableObject.SetActive(true);
        }
    }
}