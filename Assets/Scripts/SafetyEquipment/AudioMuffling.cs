using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMuffling : MonoBehaviour
{
    AudioLowPassFilter lowPassFilter;
    [Range(500, 2000)]
    [SerializeField] float cutoffFrequency = 1200f;

    void Start()
    {
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();

            lowPassFilter.cutoffFrequency = cutoffFrequency;
            lowPassFilter.enabled = false;
        }
    }

    public void MuffleSound() {
        lowPassFilter.enabled = true;
    }

    public void ResetSound()
    {
        lowPassFilter.enabled = false;
    }
}
