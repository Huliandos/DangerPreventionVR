using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Challenges
{
    /// <summary>
    /// Timer Class for counting down the remaining time for a challenge for the user
    /// </summary>
    public class Timer : MonoBehaviour
    {
        #region Audio variables
        [SerializeField] [Tooltip("HAS to be same length as below")]
        AudioClip[] timerAnnouncements;
        [SerializeField] [Tooltip("At which points in time the different timer announcements should be called in SECONDS. HAS to be same length as above")]
        int[] timerAnnouncementTimestamps;
        bool[] announcementPlayed;
        AudioSource audioSource;

        int announcementCounter = 0;

        #endregion

        [SerializeField] private TMP_Text[] secondsTextDisplays;
        [SerializeField] private TMP_Text[] minutesTextDisplays;
        [SerializeField] private Text UIsecondsText;
        [SerializeField] private Text UIminutesText;
        [SerializeField] private float challengeEpochTimeSeconds = 10f;
        [SerializeField] private float currentTime = 10f;
        [SerializeField] private float countDownSpeed = 1f;
        
        public UnityEvent onTimeOver = new UnityEvent();

        public delegate void ResetTimerDelegate();
        public delegate void StartTimerDelegate();
        
        public static ResetTimerDelegate ResetTimer = null;
        public static StartTimerDelegate StartTimer = null;

        private void Start()
        {
            announcementPlayed = new bool[timerAnnouncements.Length];
            audioSource = GetComponent<AudioSource>();
        }

        public float ChallengeEpochTimeSeconds
        {
            set => challengeEpochTimeSeconds = value;
        }

        public float CurrentTime
        {
            get => currentTime;
            set {
                currentTime = value;
                foreach(TMP_Text minutesTextDisplay in minutesTextDisplays)
                    minutesTextDisplay.text = Mathf.Floor(currentTime / 60).ToString("00");
                UIminutesText.text = Mathf.Floor(currentTime / 60).ToString("00");
                foreach (TMP_Text secondsTextDisplay in secondsTextDisplays)
                    secondsTextDisplay.text = Mathf.FloorToInt(currentTime % 60).ToString("00");
                UIsecondsText.text = Mathf.FloorToInt(currentTime % 60).ToString("00");
            } 
        }

        /// <summary>
        /// sets delegates to get the time called from any context from outside
        /// </summary>
        void Awake()
        {
            StartTimer = StartTimerCoroutine;
            ResetTimer = () => CurrentTime = challengeEpochTimeSeconds;
            ResetTimer.Invoke();
        }

        void Update()
        {
            
        }

        /// <summary>
        /// starts Timer component to countdown the given time
        /// </summary>
        public void StartTimerCoroutine() => StartCoroutine(CountDownTimer(onTimeOver.Invoke));
        
        private IEnumerator CountDownTimer(Action callback)
        {
            while (CurrentTime > 0f)
            {
                CurrentTime -= countDownSpeed * Time.deltaTime;

                if (currentTime <= timerAnnouncementTimestamps[announcementCounter]) {
                    audioSource.clip = timerAnnouncements[announcementCounter];
                    audioSource.Play();
                    announcementCounter++;
                }

                yield return null;
            }

            CurrentTime = 0f;
            callback?.Invoke();
        }
        
    }
}
