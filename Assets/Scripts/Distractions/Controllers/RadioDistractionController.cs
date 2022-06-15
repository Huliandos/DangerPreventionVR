using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ToDo: Delete. Becomes obsolete once new system is reviewed
public class RadioDistractionController : MonoBehaviour
{
    [SerializeField]
    AudioClip[] noise, newsFlash;

    RadioController radioController;

    private void Start()
    {
        radioController = GetComponent<RadioController>();
    }

    // Update is called once per frame
    void Update()
    {
        //ToDo: Remove this later. This is for testing only
        if (Input.GetKeyDown(KeyCode.A)) {
            chooseRandomDistraction();
        }
    }

    public void chooseRandomDistraction() {
        int randomNum = Random.Range(0, 2); //random between 0 and 1
        AudioClip randomClip = null;

        //noise
        if (randomNum == 0)
        {
            randomClip = noise[Random.Range(0, noise.Length)];
        }
        //newsFlash
        else
        {
            randomClip = newsFlash[Random.Range(0, newsFlash.Length)];
        }

        radioController.SwitchSong(randomClip);
    }
}
