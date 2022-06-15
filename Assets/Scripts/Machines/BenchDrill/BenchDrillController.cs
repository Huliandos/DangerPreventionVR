using Distractions.Management.EventSystem;
using Distractions.Management.EventSystem.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;
using VRTK.GrabAttachMechanics;

public class BenchDrillController : MonoBehaviour
{
    #region private fields

    private VRTK_ArtificialRotator operatingHandle_artRot;
    private VRTK_ArtificialRotator lightSwitch_artRot;
    private VRTK_ArtificialPusher powerOnButton_artPush;
    private VRTK_ArtificialPusher emergencyStopButton_artPush;

    private int drillingOrientation = 1;
    private float drillingAngleSpeed;
    private float goalAnimValue;
    private bool powerButtonWasPressed = false;
    private bool emergencyStopWasPressed = false;

    private bool ongoingDistraction = true;
    private float distractionFireCooldown = 1f;
    private float distractionTimer;

    #endregion

    #region serialized fields

    [FoldoutGroup("Drill Chuck", expanded: true)] [SerializeField] private DrillChuckController dc_controller;

    [FoldoutGroup("Operating Handle", expanded: true)] [SerializeField] GameObject operatingHandle_Parent;

    [FoldoutGroup("Light Switch", expanded: true)] [SerializeField] private GameObject lightSwitch_Parent;
    [FoldoutGroup("Light Switch")] [SerializeField] private Light workLight;

    [FoldoutGroup("Power Button", expanded: true)] [SerializeField] private GameObject powerOnButton_Parent;

    [FoldoutGroup("Emergency Stop", expanded: true)] [SerializeField] private GameObject emergencyStopButton_Parent;

    [FoldoutGroup("Config")] [SerializeField] float drillingRPS;

    #endregion

    #region public fields

    [HideInInspector] public bool drilling = false;
    [HideInInspector] public bool drillBitImproperlyTightened = false;

    #endregion

    private void Awake()
    {
        operatingHandle_artRot = (operatingHandle_artRot == null ? operatingHandle_Parent.GetComponent<VRTK_ArtificialRotator>() : operatingHandle_artRot);
        operatingHandle_artRot.ValueChanged += ValueChanged;
        operatingHandle_artRot.RestingPointReached += OperatingHandleReset;

        lightSwitch_artRot = (lightSwitch_artRot == null ? lightSwitch_Parent.GetComponent<VRTK_ArtificialRotator>() : lightSwitch_artRot);
        lightSwitch_artRot.MaxLimitReached += MaxLimitReached;
        lightSwitch_artRot.MinLimitReached += MinLimitReached;

        powerOnButton_artPush = (powerOnButton_artPush == null ? powerOnButton_Parent.GetComponent<VRTK_ArtificialPusher>() : powerOnButton_artPush);
        powerOnButton_artPush.MaxLimitReached += MaxLimitReached;

        emergencyStopButton_artPush = (emergencyStopButton_artPush == null ? emergencyStopButton_Parent.GetComponent<VRTK_ArtificialPusher>() : emergencyStopButton_artPush);
        emergencyStopButton_artPush.MaxLimitReached += MaxLimitReached;

        distractionTimer = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (workLight != null) workLight.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (powerButtonWasPressed && !emergencyStopWasPressed)
        {
            drilling = true;
            drillingAngleSpeed = drillingOrientation * drillingRPS * Time.deltaTime;
            dc_controller.dc.transform.rotation *= Quaternion.AngleAxis(drillingAngleSpeed, Vector3.up);
        }
        else drilling = false;

        dc_controller.dc.GetComponentInParent<Animator>().SetFloat("NormalizedDrillChuckPosition",
            Mathf.Lerp(dc_controller.dc.GetComponentInParent<Animator>().GetFloat("NormalizedDrillChuckPosition"), goalAnimValue, .05f));
    }

    protected virtual void ValueChanged(object sender, ControllableEventArgs e)
    {
        if (!ongoingDistraction && Time.time >= distractionTimer + distractionFireCooldown)
        {
            Debug.Log("Start if entered");

            distractionTimer = Time.time;

            DistractionEventSystem.RaiseDistractionEvent(Reason.UsageBenchDrill, DistractionEventType.Start);
            ongoingDistraction = true;
        }

        if (sender.Equals(operatingHandle_artRot)) goalAnimValue = operatingHandle_artRot.GetNormalizedValue();
    }
    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if (sender.Equals(lightSwitch_artRot)) workLight.enabled = true;

        if (sender.Equals(powerOnButton_artPush))
        {
            powerOnButton_artPush.SetStayPressed(true);
            powerButtonWasPressed = true;
            gameObject.GetComponent<AudioDrillController>().DrillStartup();

            if (emergencyStopWasPressed) emergencyStopWasPressed = false;
        }

        if (sender.Equals(emergencyStopButton_artPush))
        {
            powerOnButton_artPush.SetPositionTarget(0f);
            powerOnButton_artPush.SetStayPressed(false);

            emergencyStopWasPressed = true;
            gameObject.GetComponent<AudioDrillController>().DrillShutdown();
        }
    }

    protected virtual void MinLimitReached(object sender, ControllableEventArgs e)
    {
        if (sender.Equals(lightSwitch_artRot)) workLight.enabled = false;
    }

    protected virtual void OperatingHandleReset(object sender, ControllableEventArgs e)
    {
        if (ongoingDistraction)  //if handle is reset to its original position
        {
            Debug.Log("Distraction Event System stopped");

            distractionTimer = Time.time;

            DistractionEventSystem.RaiseDistractionEvent(Reason.UsageBenchDrill, DistractionEventType.End);
            ongoingDistraction = false;
        }
    }
}
