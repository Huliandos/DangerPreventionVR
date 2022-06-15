using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Challenges
{
    public class ChallengeManager : MonoBehaviour
    {
        [SerializeField] private Transform ExerciseAnchor;
        [SerializeField] private ScoreDisplay scoreDisplay;
        [SerializeField] public List<GameObject> exerciseIterations;

        //ToDo: This shouldn't be final
        [SerializeField] Leaderboard leaderboard;
        [SerializeField] TMPro.TMP_Text totalScoreTextfield;

        public delegate void ManualReviewDelegate([ItemCanBeNull] Dictionary<GameObject, ChallengeDataResult> mappedObjects);
        public delegate void TransmitResults(List<float> results);
        
        public static ManualReviewDelegate manualReviewDelegate = null;
        public static TransmitResults OnTransmitResults = null;

        public ExerciseController CurrentExerciseController { get; private set; }

        #region InputBools

        private bool timerStarted;
        private bool checkStarted;

        #endregion
        

        public int iterationCounter = 0;
        public GameObject lastChallengeIterationTemplate;

        private void Start()
        {
            SpawnNextChallengeIteration();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.T) && !timerStarted)
            {
                timerStarted = true;
                Timer.StartTimer.Invoke();
            }

            if (Input.GetKey(KeyCode.Z))
            {
                Timer.ResetTimer.Invoke();
            }

            if (Input.GetKeyUp(KeyCode.M) && !checkStarted)
            {
                checkStarted = true;
                //TODO: on button press call this method
                CurrentExerciseController.OnUserSubmit(EndIteration);
            }
        }

        public void TimeOverInvoke()
        {
            leaderboard.StoreScore(Convert.ToInt32(float.Parse(totalScoreTextfield.text)));

            Debug.Log("time Over!");
            timerStarted = false;
        }

        public void EndIteration(float finalResult)
        {
            ShowResults(finalResult);
            SpawnNextChallengeIteration();
        }

        private void ShowResults(float finalResult)
        {
            Debug.Log("Final Result: " + finalResult);
            checkStarted = false;

            //scoreDisplay.FinalResultDisplay = finalResult;
        }

        /// <summary>
        /// Destroys old challenge objects and instantiates new ones
        /// </summary>
        private void SpawnNextChallengeIteration()
        {
            if (lastChallengeIterationTemplate != null)
            {
                CurrentExerciseController.AcceptingCalls = false;
                lastChallengeIterationTemplate.SetActive(false);
            }

            if (iterationCounter >= exerciseIterations.Capacity) return;

            lastChallengeIterationTemplate =
                Instantiate(exerciseIterations[iterationCounter], ExerciseAnchor.position, Quaternion.identity);
            CurrentExerciseController = lastChallengeIterationTemplate.GetComponent<ExerciseController>();
            lastChallengeIterationTemplate.transform.SetParentUnscaled(ExerciseAnchor);
            
            iterationCounter++;
        }
    }
}