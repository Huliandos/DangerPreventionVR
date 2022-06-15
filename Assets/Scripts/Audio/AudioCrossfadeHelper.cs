using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCrossfadeHelper : MonoBehaviour
{
    public IEnumerator LoopAudioClip(AudioSource ASOne, AudioSource ASTwo, AudioClip clip, float fadeDuration) {
        float clipLength = clip.length;

        if (clipLength - fadeDuration >= 0)
        {
            yield return new WaitForSeconds(clipLength - fadeDuration);
            StartCoroutine(LoopAudioClip(ASOne, ASTwo, clip, fadeDuration)); //indefinitely calls itself until bein stopped from other class

            if (ASOne.isPlaying)
            {
                ASTwo.clip = clip;
                ASTwo.Play();
                ASTwo.volume = 0;
                ASTwo.outputAudioMixerGroup = ASOne.outputAudioMixerGroup;  //copy mixer group from already playing Audio Source

                float startTime = Time.time;
                //fade out of already playing Audio Source and fade in Audio Source to play
                while (ASOne.volume > 0 && ASTwo.volume < 1)
                {
                    if (fadeDuration == 0)
                    {
                        ASOne.volume = 0;
                        ASTwo.volume = 1;
                        break;  //break, to prevent division by  zero
                    }
                    float elapsedTime = Time.time - startTime;

                    ASOne.volume = Mathf.Clamp01(Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
                    if (ASOne.volume < 0) ASOne.volume = 0; //clamping

                    ASTwo.volume = Mathf.Clamp01(Mathf.Lerp(.6f, 1, elapsedTime / fadeDuration));
                    if (ASTwo.volume > 1) ASTwo.volume = 1; //clamping
                    yield return null;
                }

                ASOne.Stop();
                ASOne.volume = 1;
            }
            else //if (ASTwo.isPlaying)
            {
                ASOne.clip = clip;
                ASOne.Play();
                ASOne.volume = 0;
                ASOne.outputAudioMixerGroup = ASTwo.outputAudioMixerGroup;  //copy mixer group from already playing Audio Source

                float startTime = Time.time;
                //fade out of already playing Audio Source and fade in Audio Source to play
                while (ASTwo.volume > 0 && ASOne.volume < 1)
                {
                    if (fadeDuration == 0)
                    {
                        ASTwo.volume = 0;
                        ASOne.volume = 1;
                        break;  //break, to prevent division by  zero
                    }
                    float elapsedTime = Time.time - startTime;

                    ASTwo.volume = Mathf.Clamp01(Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
                    if (ASTwo.volume < 0) ASTwo.volume = 0; //clamping

                    ASOne.volume = Mathf.Clamp01(Mathf.Lerp(.6f, 1, elapsedTime / fadeDuration));
                    if (ASOne.volume > 1) ASOne.volume = 1; //clamping
                    yield return null;
                }
                
                ASTwo.Stop();
                ASTwo.volume = 1;
            }
            //ToDo: add case where both audio sources are playing something. E.g. in the middle of a fade
        }
        else
        {
            //ToDo: return Error Code
            yield return null;
        }
    }

    public IEnumerator connectAudioClips(AudioSource ASOne, AudioSource ASTwo, AudioClip[] clips, float fadeDuration, float playingClipLength) {
        float clipLength = playingClipLength;

        if (clipLength - fadeDuration >= 0)
        {
            yield return new WaitForSeconds(clipLength - fadeDuration);
            int clipNum = Random.Range(0, clips.Length);
            StartCoroutine(connectAudioClips(ASOne, ASTwo, clips, fadeDuration, clips[clipNum].length)); //indefinitely calls itself until bein stopped from other class

            if (ASOne.isPlaying)
            {
                ASTwo.clip = clips[clipNum];
                ASTwo.Play();
                ASTwo.volume = 0;
                ASTwo.outputAudioMixerGroup = ASOne.outputAudioMixerGroup;  //copy mixer group from already playing Audio Source

                float startTime = Time.time;
                //fade out of already playing Audio Source and fade in Audio Source to play
                while (ASOne.volume > 0 && ASTwo.volume < 1)
                {
                    if (fadeDuration == 0)
                    {
                        ASOne.volume = 0;
                        ASTwo.volume = 1;
                        break;  //break, to prevent division by  zero
                    }
                    float elapsedTime = Time.time - startTime;

                    ASOne.volume = Mathf.Clamp01(Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
                    if (ASOne.volume < 0) ASOne.volume = 0; //clamping

                    ASTwo.volume = Mathf.Clamp01(Mathf.Lerp(0, 1, elapsedTime / fadeDuration));
                    if (ASTwo.volume > 1) ASTwo.volume = 1; //clamping
                    yield return null;
                }

                ASOne.Stop();
                ASOne.volume = 1;
            }
            else //if (ASTwo.isPlaying)
            {
                ASOne.clip = clips[clipNum];
                ASOne.Play();
                ASOne.volume = 0;
                ASOne.outputAudioMixerGroup = ASTwo.outputAudioMixerGroup;  //copy mixer group from already playing Audio Source

                float startTime = Time.time;
                //fade out of already playing Audio Source and fade in Audio Source to play
                while (ASTwo.volume > 0 && ASOne.volume < 1)
                {
                    if (fadeDuration == 0)
                    {
                        ASTwo.volume = 0;
                        ASOne.volume = 1;
                        break;  //break, to prevent division by  zero
                    }
                    float elapsedTime = Time.time - startTime;

                    ASTwo.volume = Mathf.Clamp01(Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
                    if (ASTwo.volume < 0) ASTwo.volume = 0; //clamping

                    ASOne.volume = Mathf.Clamp01(Mathf.Lerp(0, 1, elapsedTime / fadeDuration));
                    if (ASOne.volume > 1) ASOne.volume = 1; //clamping
                    yield return null;
                }

                ASTwo.Stop();
                ASTwo.volume = 1;
            }
            //ToDo: add case where both audio sources are playing something. E.g. in the middle of a fade
        }
        else
        {
            //ToDo: return Error Code
            yield return null;
        }
    }
}
