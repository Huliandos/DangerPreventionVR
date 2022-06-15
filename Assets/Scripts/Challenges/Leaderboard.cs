using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField]
    string playerName;

    int firstPlaceScore, secondPlaceScore, thirdPlaceScore;
    string firstPlaceString, secondPlaceString, thirdPlaceString;

    string firstPlaceKey = "first", secondPlaceKey = "second", thirdPlaceKey = "third";

    [SerializeField]
    TMP_Text firstPlaceNameText, secondPlaceNameText, thirdPlaceNameText, firstPlaceNameScore, secondPlaceNameScore, thirdPlaceNameScore;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetString(firstPlaceKey) != "") {
            string[] str = PlayerPrefs.GetString(firstPlaceKey).Split(' ');
            firstPlaceString = str[0];
            firstPlaceScore = int.Parse(str[str.Length-1]);

            firstPlaceNameText.text = firstPlaceString;
            firstPlaceNameScore.text = firstPlaceScore.ToString();
        }
        if (PlayerPrefs.GetString(secondPlaceKey) != "")
        {
            string[] str = PlayerPrefs.GetString(secondPlaceKey).Split(' ');
            secondPlaceString = str[0];
            secondPlaceScore = int.Parse(str[str.Length - 1]);

            secondPlaceNameText.text = secondPlaceString;
            secondPlaceNameScore.text = secondPlaceScore.ToString();
        }
        if (PlayerPrefs.GetString(thirdPlaceKey) != "")
        {
            string[] str = PlayerPrefs.GetString(thirdPlaceKey).Split(' ');
            thirdPlaceString = str[0];
            thirdPlaceScore = int.Parse(str[str.Length - 1]);


            thirdPlaceNameText.text = thirdPlaceString;
            thirdPlaceNameScore.text = thirdPlaceScore.ToString();
        }
    }

    public void StoreScore(int score) {
        if (score > firstPlaceScore) {
            thirdPlaceString = secondPlaceString;
            thirdPlaceScore = secondPlaceScore;

            secondPlaceString = firstPlaceString;
            secondPlaceScore = firstPlaceScore;

            firstPlaceString = playerName;
            firstPlaceScore = score;
        }
        else if (score > secondPlaceScore)
        {
            thirdPlaceString = secondPlaceString;
            thirdPlaceScore = secondPlaceScore;

            secondPlaceString = playerName;
            secondPlaceScore = score;
        }
        else if (score > thirdPlaceScore)
        {
            thirdPlaceString = playerName;
            thirdPlaceScore = score;
        }

        PlayerPrefs.SetString(firstPlaceKey, firstPlaceString + " " + firstPlaceScore);
        PlayerPrefs.SetString(secondPlaceKey, secondPlaceString + " " + secondPlaceScore);
        PlayerPrefs.SetString(thirdPlaceKey, thirdPlaceString + " " + thirdPlaceScore);


        firstPlaceNameText.text = firstPlaceString;
        firstPlaceNameScore.text = firstPlaceScore.ToString();

        secondPlaceNameText.text = secondPlaceString;
        secondPlaceNameScore.text = secondPlaceScore.ToString();
        
        thirdPlaceNameText.text = thirdPlaceString;
        thirdPlaceNameScore.text = thirdPlaceScore.ToString();
        
    }
}
