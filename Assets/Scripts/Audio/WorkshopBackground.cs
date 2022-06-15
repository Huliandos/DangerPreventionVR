using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopBackground : MonoBehaviour
{
    [SerializeField]
    AudioClip[] workshopClips;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource ASOne = GetComponents<AudioSource>()[0];
        AudioSource ASTwo = GetComponents<AudioSource>()[1];

        int clipNum = Random.Range(0, workshopClips.Length);
        ASOne.clip = workshopClips[clipNum];
        ASOne.Play();

        StartCoroutine(GetComponent<AudioCrossfadeHelper>().connectAudioClips(ASOne, ASTwo, workshopClips, 1, workshopClips[clipNum].length));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
