using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioController : MonoBehaviour
{
    [SerializeField]
    AudioClip[] playlist;
    AudioSource localAudioSource;

    int clipPlaying;

    // Start is called before the first frame update
    void Start()
    {
        localAudioSource = GetComponent<AudioSource>();
        clipPlaying = Random.Range(0, playlist.Length);

        AudioClip clipToPlay = playlist[clipPlaying];

        localAudioSource.clip = clipToPlay;
        localAudioSource.Play();

        StartCoroutine(StartNextSong(clipToPlay.length));
    }

    IEnumerator StartNextSong(float clipLength) {
        yield return new WaitForSeconds(clipLength);

        SwitchSong();
    }

    public void SwitchSong(AudioClip distractionClip = null) {
        StopAllCoroutines();    //resets the coroutine if the playing songs get interrupted by a disctration

        AudioClip clipToPlay = distractionClip;

        //if no disctracrion clip has been passed into this method call
        if (clipToPlay == null) { 
        //Potential addition: consider adding short static sound while swapping songs
            int nextClipPlaying = Random.Range(0, playlist.Length);
            while (nextClipPlaying == clipPlaying)
                nextClipPlaying = Random.Range(0, playlist.Length);
        

            clipPlaying = nextClipPlaying;
            clipToPlay = playlist[clipPlaying];
        }
        
        localAudioSource.clip = clipToPlay;
        localAudioSource.Play();

        StartCoroutine(StartNextSong(clipToPlay.length));
    }

    public void SilenceRadio() {
        StopAllCoroutines();

        StartCoroutine(SilenceRadioCoroutine());
    }

    IEnumerator SilenceRadioCoroutine()
    {
        localAudioSource.Stop();

        yield return new WaitForSeconds(Random.Range(10f, 15f));    //wait random amount of time between 10

        SwitchSong();
    }
}
