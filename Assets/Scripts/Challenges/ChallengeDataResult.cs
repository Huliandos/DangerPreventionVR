using System;
using UnityEngine;

namespace Challenges
{
    public class ChallengeDataResult
    {
        public static float penaltyScore = 100f;
        
        private float ratioDifference;
        private float encapsulationIncrease;

        private float score;

        private GameObject referencedGameObject;

        public float Score
        {
            get
            {
                ratioDifference *= 3;
                float score = ratioDifference * (0.1f * Mathf.Exp(encapsulationIncrease));
                return score;
            }
        }

        public GameObject ReferencedGameObject
        {
            get => referencedGameObject;
            set => referencedGameObject = value;
        }

        /// <summary>
        /// if default constructor used - it is counted as no Object was evaluated
        /// </summary>
        public ChallengeDataResult()
        {
            ratioDifference = penaltyScore;
            encapsulationIncrease = penaltyScore;
        }

        public ChallengeDataResult(float ratioDifference, float encapsulationIncrease)
        {
            this.ratioDifference = ratioDifference;
            this.encapsulationIncrease = encapsulationIncrease;
        }
    }
}