using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class EarBeeping : MonoBehaviour
{
    #region private fields

    private PlayerController playerController;
    private BenchDrillController benchDrillController;
    private AudioSource ASOne, ASTwo;
    private bool coroutineStarted, startBeeping;

    #endregion

    #region serialized fields

    [SerializeField] AudioMixer SFXMixer;
    [SerializeField] AudioClip beeping;
    [SerializeField] float timeUntilHearingIsDamaged, timeUntilBeepingIsMaxVolume;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (benchDrillController == null) benchDrillController = FindObjectOfType<BenchDrillController>();

        //disable this, if the needed components aren't present in the scene
        if (playerController == null || benchDrillController == null) this.enabled = false;

        ASOne = GetComponents<AudioSource>()[0];
        ASTwo = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (startBeeping)
        {
            StartBeepingLoop(true);
        }
        //When Drill is drilling and Earmuffs aren't put on
        else if (!coroutineStarted && benchDrillController.drilling && !playerController.EarmuffsAreOn)
        {
            StartCoroutine("EarsExposed");
            coroutineStarted = true;
        }
        else if (coroutineStarted && (!benchDrillController.drilling || playerController.EarmuffsAreOn))
        {
            coroutineStarted = false;
            StopAllCoroutines();
        }
    }

    IEnumerator EarsExposed()
    {
        yield return new WaitForSeconds(timeUntilHearingIsDamaged);

        if (benchDrillController.drilling && !playerController.EarmuffsAreOn)
        {
            startBeeping = true;
        }

        coroutineStarted = false;
        StopAllCoroutines();
    }

    public void StartBeepingLoop(bool withFadeOut)
    {
        StopAllCoroutines();
        //setting crossfade
        ASOne.clip = beeping;
        ASOne.Play();

        AudioMixerSnapshot fromSnapshot = SFXMixer.FindSnapshot("Default"); //Startup sound
        AudioMixerSnapshot toSnapshot = SFXMixer.FindSnapshot("Numb");   //Neutral Sound

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
        float[] weights = new float[2] { 0, 1 };    //determines how much percent of each snapshot should be reached after Crossfade is compeleted

        SFXMixer.TransitionToSnapshots(snapshots, weights, timeUntilBeepingIsMaxVolume);   //last parameter is time for an Audio fade to happen

        StartCoroutine(GetComponent<AudioCrossfadeHelper>().LoopAudioClip(ASOne, ASTwo, beeping, 1));

        this.enabled = false;

        if (withFadeOut)
        {
            StartCoroutine(StartFadeOut());
        }
    }

    IEnumerator StartFadeOut()
    {
        yield return new WaitForSeconds(timeUntilBeepingIsMaxVolume);
        GameController.Instance.GameOver(Injury.Tinnitus);
    }
}
