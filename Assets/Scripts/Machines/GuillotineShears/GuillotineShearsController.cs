using Sirenix.OdinInspector;
using UnityEngine;
using VRTK;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class GuillotineShearsController : MonoBehaviour
{
    #region private fields

    private VRTK_ArtificialRotator drehknopf_artRot;
    private VRTK_ArtificialPusher startknopf_artPush;
    private VRTK_ArtificialPusher notknopf_artPush;
    private float origPressedDistance;

    #endregion

    #region serialized fields

    [Title("Start Switch")]
    [SceneObjectsOnly]
    [SerializeField] GameObject drehknopf_Parent;
    [SerializeField] public GameObject startknopf_Parent;

    [Title("Start Button")]
    [SceneObjectsOnly]
    [SerializeField] public float startButtonCooldown = 3f;

    [Title("Emergency Stop Button")]
    [SceneObjectsOnly]
    [SerializeField] GameObject notknopf_Parent;


    #endregion

    #region public fields

    [HideInInspector] public bool startButtonUnlocked = false;
    [HideInInspector] public bool machineIsBroken = false;

    #endregion

    private void Awake()
    {
        drehknopf_artRot = (drehknopf_artRot == null ? drehknopf_Parent.GetComponent<VRTK_ArtificialRotator>() : drehknopf_artRot);
        drehknopf_artRot.MinLimitReached += MinLimitReached;
        drehknopf_artRot.MinLimitExited += MinLimitExited;

        startknopf_artPush = (startknopf_artPush == null ? startknopf_Parent.GetComponent<VRTK_ArtificialPusher>() : startknopf_artPush);
        origPressedDistance = startknopf_artPush.pressedDistance;
        startknopf_artPush.pressedDistance = 0f;

        notknopf_artPush = (notknopf_artPush == null ? notknopf_Parent.GetComponent<VRTK_ArtificialPusher>() : notknopf_artPush);
        notknopf_artPush.MaxLimitReached += MaxLimitReached;
    }

    private void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if (sender.Equals(notknopf_artPush))
        {
            if (startknopf_Parent.GetComponentInChildren<AudioSource>().isPlaying)
            {
                startknopf_Parent.GetComponentInChildren<AudioSource>().Stop();
            }

            if (machineIsBroken) notknopf_artPush.SetStayPressed(true);
        }
    }

    private void MinLimitReached(object sender, ControllableEventArgs e)
    {
        if (!machineIsBroken)
        {
            startButtonUnlocked = true;
            startknopf_artPush.gameObject.GetComponent<VRTK_InteractHaptics>().enabled = true;
            startknopf_artPush.pressedDistance = origPressedDistance;
        }
    }

    private void MinLimitExited(object sender, ControllableEventArgs e)
    {
        startButtonUnlocked = false;
        startknopf_artPush.gameObject.GetComponent<VRTK_InteractHaptics>().enabled = false;
        startknopf_artPush.pressedDistance = 0f;
    }
}
