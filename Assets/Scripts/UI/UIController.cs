using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Distractions.Management.EventSystem.Listener.Hotspots;

public class UIController : MonoBehaviour
{
    InvokableHotspot[] allHotspots;
    InvokableHotspot activeHotspot;

    [SerializeField]
    Dropdown dropdown;

    [SerializeField]
    Button[] FlyButtons;

    [SerializeField]
    Button[] NPCButtons;

    [SerializeField]
    Button[] LightButtons;

    [SerializeField]
    Button[] RadioButtons;

    [SerializeField]
    Button[] BrokenMachineButtons;

    bool buttonReleased;

    public enum buttonType { FLY=0, FLY_ON_MACHINE=1, NPC=2, LIGHT = 3, RADIO_NOISE = 4, RADIO_SILENCE = 5, GUILLOTINE_BROKEN = 6, TIMER_START = 7, TIMER_RESET = 8 };

    // Start is called before the first frame update
    void Start()
    {
        allHotspots = FindObjectsOfType<InvokableHotspot>();

        activeHotspot = allHotspots[0];

        setUsableButtons();

        List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();

        foreach (InvokableHotspot hotspot in allHotspots) {
            Dropdown.OptionData optionData = new Dropdown.OptionData();

            optionData.text = hotspot.name.Split('_')[1];   //Cutoff "InvokableHotspot_" part
            optionDataList.Add(optionData);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionDataList);

        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    // Update is called once per frame
    void Update()
    {
        //This is for testing only!
        #region fly
        if (activeHotspot.flyDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.Q))  //player head
        {
            activeHotspot.FlyHeadDistraction();

            buttonReleased = false;
        }
        else if (activeHotspot.flyDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.A)) //machine
        {
            activeHotspot.FlyMachineDistraction();

            buttonReleased = false;
        }
        #endregion
        //LIGHT FLICKER
        else if (activeHotspot.lightFlickerDistractionInvokables.Length > 0 && activeHotspot.lightFlickerDistractionInvokables[0] != null && buttonReleased && Input.GetKeyDown(KeyCode.W))
        {
            activeHotspot.LightFlickerDistraction();

            buttonReleased = false;
        }
        #region radio
        else if (activeHotspot.radioDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.E)) //random noise
        {
            activeHotspot.RadioRandomNoise();

            buttonReleased = false;
        }
        else if (activeHotspot.radioDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.D)) //silence
        {
            activeHotspot.RadioSilence();

            buttonReleased = false;
        }
        #endregion
        //NPC
        else if (activeHotspot.nPCJumpscareDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.R))
        {
            activeHotspot.TriggerNPC();

            buttonReleased = false;
        }
        //GUILLOTINE SHEARS
        else if (activeHotspot.guillotineShearsDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.U))
        {
            activeHotspot.DestroyGuillotineSheer();

            buttonReleased = false;
        }
        else
        {
            buttonReleased = true;
        }
    }

    void DropdownValueChanged(Dropdown change)
    {
        activeHotspot = allHotspots[change.value];

        setUsableButtons();
    }

    void setUsableButtons()
    {
        if (activeHotspot.flyDistractionInvokable == null)
        {
            foreach (Button button in FlyButtons) button.interactable = false;
        }
        else
        {
            foreach (Button button in FlyButtons) button.interactable = true;
        }
        if (activeHotspot.lightFlickerDistractionInvokables == null || activeHotspot.lightFlickerDistractionInvokables.Length == 0)
        {
            foreach (Button button in LightButtons) button.interactable = false;
        }
        else
        {
            foreach (Button button in LightButtons) button.interactable = true;
        }
        if (activeHotspot.radioDistractionInvokable == null)
        {
            foreach (Button button in RadioButtons) button.interactable = false;
        }
        else
        {
            foreach (Button button in RadioButtons) button.interactable = true;
        }
        if (activeHotspot.nPCJumpscareDistractionInvokable == null)
        {
            foreach (Button button in NPCButtons) button.interactable = false;
        }
        else
        {
            foreach (Button button in NPCButtons) button.interactable = true;
        }
        if (activeHotspot.guillotineShearsDistractionInvokable == null)
        {
            foreach (Button button in BrokenMachineButtons) button.interactable = false;
        }
        else
        {
            foreach (Button button in BrokenMachineButtons) button.interactable = true;
        }
    }

    public void TriggeredButton(int enumValue) {
        //This is for testing only!

        switch ((buttonType)enumValue)
        {
            case (buttonType.FLY):
                activeHotspot.FlyHeadDistraction();
                break;
            case (buttonType.FLY_ON_MACHINE):
                activeHotspot.FlyMachineDistraction();
                break;
            case (buttonType.NPC):
                activeHotspot.TriggerNPC();
                break;
            case (buttonType.LIGHT):
                activeHotspot.LightFlickerDistraction();
                break;
            case (buttonType.RADIO_NOISE):
                activeHotspot.RadioRandomNoise();
                break;
            case (buttonType.RADIO_SILENCE):
                activeHotspot.RadioSilence();
                break;
            case (buttonType.GUILLOTINE_BROKEN):
                activeHotspot.DestroyGuillotineSheer();
                break;
            case (buttonType.TIMER_START):
                Challenges.Timer.StartTimer.Invoke();
                break;
            case (buttonType.TIMER_RESET):
                Challenges.Timer.ResetTimer.Invoke();
                break;
            default:
                break;
        }
    }
}
