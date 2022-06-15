using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioDrillController : MonoBehaviour
{
    #region private fields

    //For smooth fading between SFX
    private string[] snapshotNames = new string[5] { "Startup", "Shutdown", "Neutral", "Wood", "Metal" };
    private int snapshotPlaying = -1;   //references index of snapshot that's playing atm. -1 means non is playing
    private float startupTime = .6f, shutdownTime = .1f, suspendTime = .5f, swapTime = .5f;
    private AudioCrossfadeHelper audioCrossfadeHelper;  //for indefinetly looping drill sound
    private bool turnedOn;

    #endregion

    #region serialized fields

    [SerializeField] AudioMixer DrillSFXMixer;
    [SerializeField] AudioSource ASOne, ASTwo;   //Two Audio Sources on one GO. For smooth fading of audio clips

    [SerializeField]
    [Tooltip("Add Audio clips in order: Startup, Shutdown, Neutral, Wood, Metal")]
    AudioClip[] audioClips; //0:Startup, 1:Shutdown, 2:Neutral, 3:Wood, 4:Metal

    [SerializeField] bool test;

    #endregion

    #region public fields

    //remove for proper implementation
    public bool swapSound;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Initializing fields
        if (ASOne == null) ASOne = GetComponents<AudioSource>()[0];
        if (ASTwo == null) ASTwo = GetComponents<AudioSource>()[1];

        audioCrossfadeHelper = GetComponent<AudioCrossfadeHelper>();
    }

    // Update is called once per frame
    void Update()
    {
        if (snapshotPlaying != -1 && (!ASOne.isPlaying && !ASTwo.isPlaying))
        {
            snapshotPlaying = -1;

            //reset audio
            AudioMixerSnapshot fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[0]); //Startup sound
            AudioMixerSnapshot toSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[0]);   //Startup Sound

            AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
            float[] weights = new float[2] { 0, 1 };    //determines how much percent of each snapshot should be reached after Crossfade is compeleted

            DrillSFXMixer.TransitionToSnapshots(snapshots, weights, 0);   //last parameter is time for an Audio fade to happen
        }

        //remove for proper implementation
        if (swapSound)
        {
            swapSound = false;

            SwapDrillingMaterial(3);
        }

        if (test)
        {
            test = false;
            DrillStartup();
        }
    }

    /// <summary>
    /// starts the Drilling Sounds with a short startup phase, that then transitions to the chosen snapshot sound in a loop. 
    /// Parse in a Value to decide which Material Sound the drill starts with, no value chooses the default "Neutral" Sound
    /// 0:Startup, 1:Shutdown, 2:Neutral, 3:Wood, 4:Metal
    /// 0 and 1 doesn't have to be set, this is only used internatally
    /// </summary>
    /// <param name="snapshotNumber">0:Startup, 1:Shutdown, 2:Neutral, 3:Wood, 4:Metal</param>
    public void DrillStartup(int snapshotNumber = 2)
    {
        //ToDo properly resetting sound
        if (ASOne.isPlaying)
        {
            //Startup Sound
            ASTwo.clip = audioClips[0];
            ASTwo.loop = false;
            ASTwo.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[0])[0];
            ASTwo.Play();

            //Neutral Loop Sound
            ASOne.clip = audioClips[snapshotNumber];
            ASOne.loop = true;
            ASOne.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[snapshotNumber])[0];
            ASOne.Play();
        }
        else
        {
            //Startup Sound
            ASOne.clip = audioClips[0];
            ASOne.loop = false;
            ASOne.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[0])[0];
            ASOne.Play();

            //Neutral Loop Sound
            ASTwo.clip = audioClips[snapshotNumber];
            ASTwo.loop = true;
            ASTwo.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[snapshotNumber])[0];
            ASTwo.Play();
        }

        //setting crossfade
        AudioMixerSnapshot fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[0]); //Startup sound
        AudioMixerSnapshot toSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[snapshotNumber]);   //Neutral Sound

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
        float[] weights = new float[2] { 0, 1 };    //determines how much percent of each snapshot should be reached after Crossfade is compeleted

        DrillSFXMixer.TransitionToSnapshots(snapshots, weights, startupTime);   //last parameter is time for an Audio fade to happen

        snapshotPlaying = snapshotNumber;
        turnedOn = true;

        //Debug.Log(audioClips[snapshotNumber].name);

        StartCoroutine(audioCrossfadeHelper.LoopAudioClip(ASOne, ASTwo, audioClips[snapshotNumber], suspendTime));
    }

    /// <summary>
    /// plays the drill shutdown sound and ends the audio loop afterwards
    /// </summary>
    public void DrillShutdown()
    {
        if (ASOne.isPlaying)
        {
            ASTwo.clip = audioClips[1];
            ASTwo.loop = false;
            ASTwo.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[1])[0];
            ASTwo.Play();

            StartCoroutine(StopAudioSourcePlayback(ASOne, shutdownTime));
        }
        else
        {
            ASOne.clip = audioClips[1];
            ASOne.loop = false;
            ASOne.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[1])[0];
            ASOne.Play();

            StartCoroutine(StopAudioSourcePlayback(ASTwo, shutdownTime));
        }

        AudioMixerSnapshot fromSnapshot;

        //find last played Snapshot if Snapshot is playing, if not assign this Snapshot to the same Snapshot as toSpanshot is referencing
        if (snapshotPlaying != -1) fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[snapshotPlaying]);
        else fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[1]);

        AudioMixerSnapshot toSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[1]);   //shutdown sound

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
        float[] weights = new float[2] { 0, 1 };    //determines how much percent of each snapshot should be reached after Crossfade is compeleted

        DrillSFXMixer.TransitionToSnapshots(snapshots, weights, shutdownTime);   //last parameter is time for an Audio fade to happen

        snapshotPlaying = 1;
        turnedOn = false;

        audioCrossfadeHelper.StopAllCoroutines();
    }

    /// <summary>
    /// Swaps the drilling sound for the chosen material 0:Startup, 1:Shutdown, 2:Neutral, 3:Wood, 4:Metal.
    /// 0 and 1 doesn't have to be set, this is only used internatally
    /// </summary>
    /// <param name="snapshotNumber">0:Startup, 1:Shutdown, 2:Neutral, 3:Wood, 4:Metal</param>
    public void SwapDrillingMaterial(int snapshotNumber)
    {
        audioCrossfadeHelper.StopAllCoroutines();   //reset Audio Looper

        //reset Audio sources
        ASOne.volume = 1;
        ASTwo.volume = 1;

        if (ASOne.isPlaying)
        {
            ASTwo.clip = audioClips[snapshotNumber];
            ASTwo.loop = true;
            ASTwo.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[snapshotNumber])[0];
            ASTwo.Play();

            StartCoroutine(StopAudioSourcePlayback(ASOne, swapTime));
        }
        else
        {
            ASOne.clip = audioClips[snapshotNumber];
            ASOne.loop = true;
            ASOne.outputAudioMixerGroup = DrillSFXMixer.FindMatchingGroups(snapshotNames[snapshotNumber])[0];
            ASOne.Play();

            StartCoroutine(StopAudioSourcePlayback(ASTwo, swapTime));
        }

        AudioMixerSnapshot fromSnapshot;

        //find last played Snapshot if Snapshot is playing, if not assign this Snapshot to the same Snapshot as toSpanshot is referencing
        if (snapshotPlaying != -1) fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[snapshotPlaying]);
        else fromSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[snapshotNumber]);

        AudioMixerSnapshot toSnapshot = DrillSFXMixer.FindSnapshot(snapshotNames[snapshotNumber]);   //shutdown sound

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
        float[] weights = new float[2] { 0, 1 };    //determines how much percent of each snapshot should be reached after Crossfade is compeleted

        DrillSFXMixer.TransitionToSnapshots(snapshots, weights, swapTime);   //last parameter is time for an Audio fade to happen

        snapshotPlaying = snapshotNumber;
        StartCoroutine(audioCrossfadeHelper.LoopAudioClip(ASOne, ASTwo, audioClips[snapshotNumber], suspendTime));
    }

    IEnumerator StopAudioSourcePlayback(AudioSource AS, float timeUntilShutdown)
    {
        yield return new WaitForSeconds(timeUntilShutdown);
        AS.Stop();
    }
}
