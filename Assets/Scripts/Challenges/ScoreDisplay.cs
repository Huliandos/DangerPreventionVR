using System;
using TMPro;
using UnityEngine;
using Utility;

namespace Challenges
{
    public enum ScoreGrade
    {
        A = 50,
        B = 40,
        C = 30,
        D = 20,
        E = 10,
        F = 0,
    }

    public enum OldGrade
    {
        A = 0,
        B = 10,
        C = 20,
        D = 30,
        E = 40,
        F = 50,
    }

    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text LastScoreDisplayText;
        [SerializeField] private TMP_Text OverallScoreDisplayText;
        [SerializeField] private TMP_Text CurrentTaskDisplayText;

        [SerializeField] private GameObject[] tasks;

        public delegate void TransmitSingleResult(float singleResult);
        public delegate void EvaluateResultManually();

        public static TransmitSingleResult onTransmitSingleResult = null;
        public static EvaluateResultManually onEvaluateResultManually = null;

        private int scoreDisplayCounter = 0;
        private float currentResult = 0;
        private float overallResult = 0;
        private int manualSingleResultCounter = 0;
        private int manualOverallResultCounter = 0;

        private ChallengeManager challengeManager;

        public float LastResultDisplayValue
        {
            get => Convert.ToSingle(LastScoreDisplayText.text);
            set => LastScoreDisplayText.text = value.ToString("00.0");
        }

        public float OverallResultDisplayValue
        {
            get => Convert.ToSingle(OverallScoreDisplayText.text);
            set => OverallScoreDisplayText.text = value.ToString("00.0");
        }

        private void Awake()
        {
            onTransmitSingleResult = AccumulateSingleResult;
            onEvaluateResultManually = EvaluateResult;
            if (LastScoreDisplayText == null) LastScoreDisplayText = GetComponentInChildren<TMP_Text>();
            if (OverallScoreDisplayText == null) OverallScoreDisplayText = GetComponentInChildren<TMP_Text>();
            if (CurrentTaskDisplayText == null) CurrentTaskDisplayText = GetComponentInChildren<TMP_Text>();
            if (challengeManager == null) challengeManager = gameObject.transform.parent.GetComponentInChildren<ChallengeManager>();
            //tasks = new GameObject[challengeManager.exerciseIterations.Count];
        }

        private void Start()
        {
            // CurrentTaskDisplayText.text = $"{challengeManager.iterationCounter + 1} / {challengeManager.exerciseIterations.Count}";
            //CurrentTaskDisplayText.text = $" 1 / {challengeManager.exerciseIterations.Count}";
        }

        private void OnDestroy()
        {
            onTransmitSingleResult = null;
            onEvaluateResultManually = null;
        }

        private void AccumulateSingleResult(float singleResult)
        {
            if (currentResult >= float.MaxValue) currentResult = 0;
            currentResult += singleResult;
            overallResult += singleResult;
            manualSingleResultCounter++;
            manualOverallResultCounter++;
        }

        private void EvaluateResult()
        {
            if (manualSingleResultCounter <= 0 || manualOverallResultCounter <= 0) return;
            LastResultDisplayValue = currentResult;
            OverallResultDisplayValue = overallResult;
            manualSingleResultCounter = 0;
            currentResult = 0;

            //CurrentTaskDisplayText.text = $"{challengeManager.iterationCounter} / {challengeManager.exerciseIterations.Count}";
            tasks[scoreDisplayCounter].transform.GetChild(0).gameObject.SetActive(false);
            tasks[scoreDisplayCounter].transform.GetChild(1).gameObject.SetActive(true);
            scoreDisplayCounter++;
        }

        public static OldGrade LookUpScoreGrade(float score)
        {
            if (score.Between((int)OldGrade.A, (int)OldGrade.B, true)) return OldGrade.A;
            if (score.Between((int)OldGrade.B, (int)OldGrade.C, true)) return OldGrade.B;
            if (score.Between((int)OldGrade.C, (int)OldGrade.D, true)) return OldGrade.C;
            if (score.Between((int)OldGrade.D, (int)OldGrade.E, true)) return OldGrade.D;
            if (score.Between((int)OldGrade.E, (int)OldGrade.F, true)) return OldGrade.E;
            return OldGrade.F;
        }
    }
}