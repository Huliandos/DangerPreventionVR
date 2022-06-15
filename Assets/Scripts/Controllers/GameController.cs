using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

[Serializable]
public enum Injury
{
    Victory = -1,
    Cutting = 0,
    Drilling = 1,
    Bonking = 2,
    Headshot = 3,
    Tinnitus = 4,
    Safety = 5
}

[Serializable]
public struct InjuryClipboardStruct
{
    public Injury injury;
    public GameObject clipboardPrefab;

    public InjuryClipboardStruct(Injury injury, GameObject clipboardPrefab)
    {
        this.injury = injury;
        this.clipboardPrefab = clipboardPrefab;
    }
}

public class GameController : MonoBehaviour
{
    #region private fields

    private static GameController instance;
    private WhiteboardController whiteboardController;
    private GameControllerDataContainer dataContainer;
    private Injury clipboardToSpawnInjury = Injury.Victory;
    static bool ongoingDeathSequence;

    #endregion

    #region serialized fields

    [SerializeField] VRTK_HeadsetFade headsetFade;
    [SerializeField] int timeUntilReload;
    [SerializeField] int shorterTimeUntilReload;
    [SerializeField] Vector3 clipboardSpawnPos, clipboardSpawnEul, playerRespawn;
    [SerializeField] private SwitchVRControllersOnStartup switchVRControllers;

    //[Tooltip("0 = CuttingClipboard, 1 = DrillingClipboard, 2 = BonkingClipboard, 3 = HeadshotClipboard, 4 = TinnitusClipboard, 5 = SafetyGlassesClipboard")]
    [SerializeField] 
    private List<InjuryClipboardStruct> clipboards;

    [Header("Avatar Parts")]
    [SerializeField] private List<GameObject> avatarParts_female = new List<GameObject>();
    [SerializeField] private List<GameObject> avatarParts_male = new List<GameObject>();
    private Dictionary<CharacterSelector.AvatarGender, List<GameObject>> female = new Dictionary<CharacterSelector.AvatarGender, List<GameObject>>();
    private Dictionary<CharacterSelector.AvatarGender, List<GameObject>> male = new Dictionary<CharacterSelector.AvatarGender, List<GameObject>>();

    [Header("Persisted Info")]
    [SerializeField] private float _avatarScalingFactor;
    [SerializeField] private CharacterSelector.AvatarGender _avatarGender;

    [Header("Relocating Info")]
    [SerializeField] float headsetFadeTime;
    [SerializeField] Transform playerSpawnpoint;
    [SerializeField] GameObject vrtkSetup;

    public float AvatarScalingFactor
    {
        get { return _avatarScalingFactor; }
        private set { _avatarScalingFactor = value; }
    }
    public CharacterSelector.AvatarGender AvatarGender
    {
        get { return _avatarGender; }
        private set { _avatarGender = value; }
    }

    #endregion

    #region properties

    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameController.Instance;
                if (instance == null)
                    Debug.LogError("GameManager not present because it was destroyed.");
            }
                
            return instance;
        }
    }

    public GameControllerDataContainer DataContainer
    {
        get
        {
            if (dataContainer == null)
                dataContainer = FindObjectOfType<GameControllerDataContainer>();
            return dataContainer;
        }
    }

    #endregion

    void Awake()
    {
        if (instance == null)
        {
            instance = this; // In first scene, make us the singleton.
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // On reload, singleton already set, so destroy duplicate.
        }

    }

    void Start()
    {
        if (clipboardToSpawnInjury != Injury.Victory)
        {
            ongoingDeathSequence = false;
            
            List<GameObject> instantiatableClipboards = clipboards
                .FindAll(entry => entry.injury == clipboardToSpawnInjury)
                .Select(entry => entry.clipboardPrefab)
                .ToList();
            instantiatableClipboards.ForEach(prefab => Instantiate(prefab, clipboardSpawnPos, Quaternion.Euler(clipboardSpawnEul)));
            
            GameObject vrtk_setup = DataContainer.ReturnVRTKSetup();
            vrtk_setup.transform.position = playerRespawn;
            StartCoroutine(WaitForWhiteBoardInit());
            StartCoroutine(WaitForCameraInit());
        }
    }

    /// <summary> Sets up the correct clipboard to spawn and initiates respawning of player
    /// <para> IDs: -1 = Victory, 0 = Cutting, 1 = Drilling, 2 = Bonking, 3 = Headshot, 4 = Tinnitus, 5 = Safety Glasses</para> 
    /// <param name="injuryId"> Shortcut ID for type of game ending condition </param>
    /// <param name="useShorterTimeUntilReload"> Flag to use shorter time until player respawn. Default is false </param>
    /// </summary>
    public void GameOver(Injury injuryId, bool useShorterTimeUntilReload = false)
    {
        if (!ongoingDeathSequence)
        {
            ongoingDeathSequence = true;

            int tempTimeUntilReload = timeUntilReload;
            if (useShorterTimeUntilReload) tempTimeUntilReload = shorterTimeUntilReload;
            clipboardToSpawnInjury = injuryId;

            StartCoroutine(DeathSequence(tempTimeUntilReload));
        }
    }

    IEnumerator DeathSequence(int timeUntilReload)
    {
        if (headsetFade == null)
        {
            Debug.Log("Error 1");
            headsetFade = DataContainer.ReturnHeadsetFade();
        }
        headsetFade.Fade(Color.black, timeUntilReload);
        yield return new WaitForSeconds(timeUntilReload);

        SceneManager.LoadScene(0);
    }
    
    private IEnumerator WaitForWhiteBoardInit()
    {
        whiteboardController = null;
        
        while ((whiteboardController = FindObjectOfType<WhiteboardController>()) == null)
        {
            yield return null;
        }
        
        whiteboardController.ShowSlide(clipboardToSpawnInjury);
    }
    
    private IEnumerator WaitForCameraInit()
    {
        Transform vrtkCameraRigTransform;
        while ((vrtkCameraRigTransform = DataContainer.ReturnHeadsetCameraRig()) == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        float differenceAngleY = Vector3.SignedAngle(vrtkCameraRigTransform.forward, new Vector3(0f, whiteboardController.transform.position.y, 0f), Vector3.up);
        vrtkCameraRigTransform.RotateAround(vrtkCameraRigTransform.position, Vector3.up, -differenceAngleY);
    }

    /// <summary>
    /// Fetches needed data from container, which would otherwise get lost after scene reload
    /// </summary>
    private void FetchReferencesFromDataContainer()
    {
        avatarParts_female = DataContainer.ReturnFemaleAvatarParts();
        avatarParts_male = DataContainer.ReturnMaleAvatarParts();
        switchVRControllers = DataContainer.ReturnSwitchVRControllersScript();
        headsetFade = DataContainer.ReturnHeadsetFade();
        vrtkSetup = DataContainer.ReturnVRTKSetup();

        female = new Dictionary<CharacterSelector.AvatarGender, List<GameObject>>();
        male = new Dictionary<CharacterSelector.AvatarGender, List<GameObject>>();

        female.Add(CharacterSelector.AvatarGender.FEMALE, avatarParts_female);
        male.Add(CharacterSelector.AvatarGender.MALE, avatarParts_male);
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.isLoaded)
        {
            FetchReferencesFromDataContainer();

            Start();
            //FadeHeadset();
            if (AvatarScalingFactor != 0) ActivateScaledAvatar(AvatarGender, AvatarScalingFactor);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            GameController.Instance.GameOver(Injury.Drilling, true);
    }

    /// <summary>
    /// Saves the gender and scaling values to reuse on respawn
    /// </summary>
    /// <param name="avatarGender"> Selected gender in character selection phase </param>
    /// <param name="scalingFactor"> Calculated up/downscale factor (based on player height) in character selection phase </param>
    public void SaveAvatarConfig(CharacterSelector.AvatarGender avatarGender, float scalingFactor)
    {
        AvatarGender = avatarGender;
        AvatarScalingFactor = scalingFactor;
    }

    /// <summary>
    /// Applies the scaling (to rig and hands) and enables the chosen avatar according to the saved gender and scaling factor
    /// </summary>
    /// <param name="avatarGender"> Selected gender in character selection phase </param>
    /// <param name="scalingFactor"> Calculated up/downscale factor (based on player height) in character selection phase </param>
    public void ActivateScaledAvatar(CharacterSelector.AvatarGender avatarGender, float scalingFactor)
    {
        //TODO: anhand gender rausfinden welche liste an GOs aktiviert werden soll
        if (female.ContainsKey(avatarGender))
        {
            //TODO: Rig skalieren
            female[avatarGender][0].transform.SetGlobalScale(new Vector3(scalingFactor, scalingFactor, scalingFactor));

            //TODO: set and reuse float = scalingFactor * 0.85f

            //TODO: Händer skalieren
            female[avatarGender][1].transform.SetGlobalScale(new Vector3(scalingFactor * 0.85f, scalingFactor * 0.85f, scalingFactor * 0.85f));
            female[avatarGender][2].transform.SetGlobalScale(new Vector3(-scalingFactor * 0.85f, scalingFactor * 0.85f, scalingFactor * 0.85f));

            //TODO: Skinned Mesh Offset anpassen
            //Entweder mit Betrag rechnen (lokal scale) oder z-achse nehmen da immer positiv
            Vector3 newScaleOffset_firstHand = new Vector3(1 - scalingFactor * 0.85f, 1 - scalingFactor * 0.85f, 1 - scalingFactor * 0.85f);
            female[avatarGender][1].gameObject.GetComponent<SkinnedMeshOffset>().SetScaleOffset(newScaleOffset_firstHand);

            Vector3 newScaleOffset_secondHand = new Vector3(1 - scalingFactor * 0.85f, 1 - scalingFactor * 0.85f, 1 - scalingFactor * 0.85f);
            female[avatarGender][2].gameObject.GetComponent<SkinnedMeshOffset>().SetScaleOffset(newScaleOffset_secondHand);

            //TODO: Alles enablen
            foreach (GameObject go in female[avatarGender])
            {
                if (!go.activeSelf) go.SetActive(true);
            }
        }
        else if (male.ContainsKey(avatarGender))
        {
            //TODO: Rig skalieren
            male[avatarGender][0].transform.SetGlobalScale(new Vector3(scalingFactor, scalingFactor, scalingFactor));

            //TODO: Händer skalieren
            male[avatarGender][1].transform.SetGlobalScale(new Vector3(scalingFactor, scalingFactor, scalingFactor));
            male[avatarGender][2].transform.SetGlobalScale(new Vector3(-scalingFactor, scalingFactor, scalingFactor));

            //TODO: Skinned Mesh Offset anpassen
            //Entweder mit Betrag rechnen (lokal scale) oder z-achse nehmen da immer positiv
            Vector3 newScaleOffset_firstHand = new Vector3(1 - scalingFactor, 1 - scalingFactor, 1 - scalingFactor);
            male[avatarGender][1].gameObject.GetComponent<SkinnedMeshOffset>().SetScaleOffset(newScaleOffset_firstHand);

            Vector3 newScaleOffset_secondHand = new Vector3(1 - scalingFactor, 1 - scalingFactor, 1 - scalingFactor);
            male[avatarGender][2].gameObject.GetComponent<SkinnedMeshOffset>().SetScaleOffset(newScaleOffset_secondHand);

            //TODO: Alles enablen
            foreach (GameObject go in male[avatarGender])
            {
                if (!go.activeSelf) go.SetActive(true);
            }
        }

        //TODO: von Controllern zu Händen wechslen
        // SwapControllerHands Script ist disabled, erst enablen wenn relocated player
        if (!switchVRControllers.enabled) switchVRControllers.enabled = true;
    }

    /// <summary>
    /// Coroutine to cover up the players repositioning with a headset fade animation
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeHeadset()
    {
        if (headsetFade == null)
        {
            Debug.Log("Error 2");
            headsetFade = DataContainer.ReturnHeadsetFade();
        }
        headsetFade.Fade(Color.black, headsetFadeTime);
        yield return new WaitForSeconds(headsetFadeTime);

        // relocating player after char selec, no respawn due to injury
        if (clipboardToSpawnInjury == Injury.Victory)
        {
            if (vrtkSetup != null) vrtkSetup.transform.position = playerSpawnpoint.position;
            else
            {
                Debug.Log("Error 3");
                vrtkSetup = DataContainer.ReturnVRTKSetup();
            }
        }

        if (AvatarScalingFactor != 0) ActivateScaledAvatar(AvatarGender, AvatarScalingFactor);
        headsetFade.Unfade(headsetFadeTime);
    }
}
