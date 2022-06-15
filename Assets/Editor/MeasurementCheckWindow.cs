using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Challenges;
using Debugging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utility;

//namespace Editor
//{
    public class MeasurementCheckWindow : EditorWindow
    {
        [SerializeField]
        private Texture cameraTexture;
        
        string scoreString = "enter valid score";
        private List<float> volumeResults = new List<float>();
        private int epochCounter = 0;
        private List<ScoreGrade> possibleGrades = new List<ScoreGrade>();


        private void Awake()
        {
            if (cameraTexture == null)
                cameraTexture = EditorGUIUtility.FindTexture("Assets/Textures/GUI/Orthographic_Camera_Projection.renderTexture");
        }

        private void OnEnable()
        {
            ChallengeManager.OnTransmitResults = (values) =>
            {
                epochCounter = 0;
                volumeResults = new List<float>(values);
            };
            
            possibleGrades = EnumExtensions.GetValues<ScoreGrade>().ToList();
        }

        [MenuItem("Window/ManualMeasurementChecker")]
        public static void OpenWindow()
        {
            EditorWindow editorWindow = GetWindow<MeasurementCheckWindow>("Manual Measurement Checker");
            
            EditorUtility.SetDirty(editorWindow);

            editorWindow.Show();
        }

        private void OnGUI()
        {
            Rect rect = EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Estimated Failure value:");
            if (volumeResults.Count > 0 && epochCounter < volumeResults.Count)
            {
                OldGrade estimatedGrade = ScoreDisplay.LookUpScoreGrade(volumeResults[epochCounter]);
                GUILayout.Label(estimatedGrade.FetchName());
            }
            else GUILayout.Label("0");
            
            EditorGUILayout.EndHorizontal();


            GUILayout.Box(cameraTexture, GUIStyle.none);
            
            Rect r = EditorGUILayout.BeginHorizontal();
            
            for (int i = possibleGrades.Count - 1; i >= 0; i--)
                CreateGradeButton(possibleGrades[i]);
            
            EditorGUILayout.EndHorizontal();
            
            scoreString = EditorGUILayout.TextField("Grade", scoreString);

            if (GUILayout.Button("Submit"))
            {
                float score ;
                bool parseable = Single.TryParse(scoreString, out score);
                if (parseable)
                {
                    MeasuredElementReceiver.manualEvaluation.Invoke(score);
                    epochCounter++;
                }
                else
                    Debug.LogWarning("Please Enter a valid value");
            }
            
            Repaint();
        }

        private void CreateGradeButton(ScoreGrade scoreGrade)
        {
            if (GUILayout.Button(scoreGrade.ToString()))
                scoreString = ((int) scoreGrade).ToString();
        }
    }
//}