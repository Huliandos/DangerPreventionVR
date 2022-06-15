using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

namespace Challenges
{
    public class ExerciseController : MonoBehaviour
    {
        public bool AcceptingCalls
        {
            get => acceptingCalls;
            set
            {
                acceptingCalls = value;
                if (registeredSnapZones.Count <= 0) return;
                registeredSnapZones.ForEach(checkableSnapZone => checkableSnapZone.AcceptingCalls = value);
            }
        }

        [SerializeField] private List<CheckableSnapZone> registeredSnapZones = new List<CheckableSnapZone>();

        private bool acceptingCalls = true;
        private List<ChallengeDataResult> measurementResults = new List<ChallengeDataResult>();
        private Action<float> onResultsEvaluatedEvent;

        /// <summary>
        /// Method to execute when User confirm he/she has completed the current Exercise
        /// </summary>
        /// <param name="onResultsEvaluated">Callback with average Result that should be executed when all single Results are evaluated</param>
        public void OnUserSubmit(Action<float> onResultsEvaluated = default)
        {
            if (!AcceptingCalls) return;
            measurementResults = new List<ChallengeDataResult>();
            onResultsEvaluatedEvent = onResultsEvaluated;
            
            foreach (CheckableSnapZone checkableSnapZone in registeredSnapZones)
            {
                checkableSnapZone.CheckMeasurements(ResultsDetermined, TransmitObjects);
            }
        }

        /// <summary>
        /// Method that is executed for every CheckableDropZone if it has checked its Result
        /// </summary>
        /// <param name="singleResult">Result from the evaluation in the dropzone</param>
        private void ResultsDetermined(ChallengeDataResult singleResult)
        {
            measurementResults.Add(singleResult);
            Debug.Log("Returned REsult : " + singleResult.Score);

            if (measurementResults.Count >= registeredSnapZones.Count)
            {
                EvaluateResults(measurementResults);
            }
        }

        /// <summary>
        /// Method for evaluation criteria on all collectively analyzed Results on every piece of the exercise
        /// </summary>
        /// <param name="calculatedResults">the list of single Results of every piece of exercise provided</param>
        private void EvaluateResults(List<ChallengeDataResult> calculatedResults)
        {
            float finalScore = 0;
            
            foreach (ChallengeDataResult dataResult in calculatedResults)
            {
                finalScore += dataResult.Score;
            }
            finalScore /= calculatedResults.Count;
            
            onResultsEvaluatedEvent.Invoke(finalScore);
        }

        #region ManualReviewing
        
        [ItemCanBeNull] private Dictionary<GameObject, ChallengeDataResult> currentIterationObjectMapping= new Dictionary<GameObject, ChallengeDataResult>();

        
        private void TransmitObjects(GameObject snapzone, ChallengeDataResult snappedObjectResult)
        {
            currentIterationObjectMapping.Add(snapzone, snappedObjectResult);

            if (currentIterationObjectMapping.Count >= registeredSnapZones.Count)
            {
                ChallengeManager.manualReviewDelegate.Invoke(currentIterationObjectMapping);

                List<float> results = currentIterationObjectMapping.Values.Select(stuff => stuff.Score).ToList();
                ChallengeManager.OnTransmitResults?.Invoke(results);
            }
        }

        #endregion
    }
}