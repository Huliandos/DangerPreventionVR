using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : MonoBehaviour
{
    #region private fields

    private PostProcessVolume postProcessVolume;

    [SerializeField] private SnapDropZonePatch earmuff_sdzPatch;

    // Needed because of set declaration
    private bool _earmuffsAreOn = false;

    #endregion

    #region public fields

    public bool SafetyGlassesAreOn { get; set; }
    public bool EarmuffsAreOn
    {
        get
        {
            return _earmuffsAreOn;
        }
        set
        {
            _earmuffsAreOn = value;
            if (FindObjectOfType<AudioMuffling>() != null)
            {
                if (EarmuffsAreOn) FindObjectOfType<AudioMuffling>().MuffleSound();

                //TODO: This should happen when grabbed Earmuffs leave SDZ -> more realism
                // HACKY
                else
                {
                    Debug.Log("Here");
                    FindObjectOfType<AudioMuffling>().ResetSound();
                }
            };
        }
    }
    public bool GlovesAreOn { get; set; }
    public bool HelmetIsOn { get; set; }

    public AudioClip heartbeatClip;
    public AudioClip headSmashClip;
    public AudioClip glassesHitClip;
    public AudioClip glassesCrackClip;
    public AudioClip eyeGougeClip;

    #endregion

    private void OnEnable()
    {
        if (postProcessVolume == null) postProcessVolume = FindObjectOfType<PostProcessVolume>();
    }

    public void PlayHeartbeatSound(bool playOnLoop)
    {
        if (playOnLoop)
        {
            GetComponent<AudioSource>().loop = true;
            GetComponent<AudioSource>().clip = heartbeatClip;
            GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(heartbeatClip);
        }
    }

    public void PlayHeadSmashSound()
    {
        GetComponent<AudioSource>().PlayOneShot(headSmashClip);
    }

    public void DisplayInjuryVFX(bool redVignetteEffect)
    {
        if (redVignetteEffect)
        {
            Color red = new Color(0.7843137f, 0f, 0f, 1f);
            postProcessVolume.profile.GetSetting<Vignette>().color.value = red;
        }
        postProcessVolume.profile.GetSetting<Vignette>().enabled.value = true;
        postProcessVolume.profile.GetSetting<Grain>().enabled.value = true;
    }

    public IEnumerator PlayEyeHitWithGlassesSound()
    {
        GetComponent<AudioSource>().PlayOneShot(glassesHitClip);
        GetComponent<AudioSource>().PlayOneShot(glassesCrackClip);

        yield return null;
    }

    public IEnumerator PlayEyeHitWithoutGlassesSound()
    {
        GetComponent<AudioSource>().PlayOneShot(glassesHitClip);
        GetComponent<AudioSource>().PlayOneShot(eyeGougeClip);

        yield return new WaitForSeconds(0.8f);

        PlayHeartbeatSound(true);
    }

}