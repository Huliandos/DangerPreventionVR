using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Models.Invokables;
using System;
using UnityEngine;

namespace Distractions.Management.EventSystem.Listener.Hotspots
{
    public class InvokableHotspot : DistractionInvokableHotspot
    {
        public FlyDistractionInvokable flyDistractionInvokable;

        public LightFlickerDistractionInvokable[] lightFlickerDistractionInvokables;

        public RadioDistractionInvokable radioDistractionInvokable;

        public NPCJumpscareDistractionInvokable nPCJumpscareDistractionInvokable;

        public GuillotineShearsDistractionInvokable guillotineShearsDistractionInvokable;

        [SerializeField, Range(0, 100)]
        int distractionProbability;

        [SerializeField]
        public bool activateKeyboardInputs;

        bool buttonReleased;

        int lastGeneratedRandomDistraction = -1;

        private void Update()
        {
            /*
            if (activateKeyboardInputs)
            {
                //This is for testing only!
                #region fly
                if (flyDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.Q))  //player head
                {
                    FlyHeadDistraction();

                    buttonReleased = false;
                }
                else if (flyDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.A)) //machine
                {
                    FlyMachineDistraction();

                    buttonReleased = false;
                }
                #endregion
                //LIGHT FLICKER
                else if (lightFlickerDistractionInvokables.Length > 0 && lightFlickerDistractionInvokables[0] != null && buttonReleased && Input.GetKeyDown(KeyCode.W))
                {
                    LightFlickerDistraction();

                    buttonReleased = false;
                }
                #region radio
                else if (radioDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.E)) //random noise
                {
                    RadioRandomNoise();

                    buttonReleased = false;
                }
                else if (radioDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.D)) //silence
                {
                    RadioSilence();

                    buttonReleased = false;
                }
                #endregion
                //NPC
                else if (nPCJumpscareDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.R))
                {
                    TriggerNPC();

                    buttonReleased = false;
                }
                //GUILLOTINE SHEARS
                else if (guillotineShearsDistractionInvokable != null && buttonReleased && Input.GetKeyDown(KeyCode.U))
                {
                    DestroyGuillotineSheer();

                    buttonReleased = false;
                }
                else
                {
                    buttonReleased = true;
                }
            }
            */
        }

        public void FlyHeadDistraction()
        {
            flyDistractionInvokable.ChooseRandomFlySpawnLocation(0);
        }

        public void FlyMachineDistraction() 
        {
            flyDistractionInvokable.ChooseRandomFlySpawnLocation(1);
        }

        public void LightFlickerDistraction() {
            lastGeneratedRandomDistraction = 1;
            DistractionType = Utility.DistractionType.Flicker;
            foreach (LightFlickerDistractionInvokable lightFlickerDistractionInvokable in lightFlickerDistractionInvokables)
            {
                lightFlickerDistractionInvokable.InvokeDistraction(new DistractionEventData().DistractionData);
            }
        }

        public void RadioRandomNoise() {
            lastGeneratedRandomDistraction = 2;
            DistractionType = Utility.DistractionType.Radio;
            radioDistractionInvokable.InvokeDistraction(new DistractionEventData().DistractionData);
        }

        public void RadioSilence()
        {
            radioDistractionInvokable.ChooseRandomRadioDistraction(true);
        }

        public void TriggerNPC() {
            lastGeneratedRandomDistraction = 3;
            DistractionType = Utility.DistractionType.Approach;
            nPCJumpscareDistractionInvokable.InvokeDistraction(new DistractionEventData().DistractionData);
        }

        public void DestroyGuillotineSheer() {
            lastGeneratedRandomDistraction = 4;
            DistractionType = Utility.DistractionType.Guillotine;
            guillotineShearsDistractionInvokable.InvokeDistraction(new DistractionEventData().DistractionData);
        }

        public override void OnEventStart(DistractionEventData distractionEventData)
        {
            System.Random rand = new System.Random();

            int randomDistractionProbability = rand.Next(0, 101); //0% -> 100%

            if (randomDistractionProbability < 100 - distractionProbability) return;    //if distraction shouldn't be called as per random generation

            int randomDistraction = rand.Next(0, 4);
            lastGeneratedRandomDistraction = randomDistraction;

            switch (randomDistraction)
            {
                case 0:
                    DistractionType = Utility.DistractionType.Fly;
                    flyDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
                    break;
                case 1:
                    DistractionType = Utility.DistractionType.Flicker;
                    foreach (LightFlickerDistractionInvokable lightFlickerDistractionInvokable in lightFlickerDistractionInvokables)
                    {
                        lightFlickerDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
                    }
                    break;
                case 2:
                    DistractionType = Utility.DistractionType.Radio;
                    radioDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
                    break;
                case 3:
                    DistractionType = Utility.DistractionType.Approach;
                    nPCJumpscareDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
                    break;
                default:
                    Console.WriteLine("random distraction generated out of bounds");
                    break;
            }

        }

        public override void OnEventEnd()
        {
            switch (lastGeneratedRandomDistraction)
            {
                case 0:
                    flyDistractionInvokable.RevokeDistraction();
                    break;
                case 1:
                    foreach (LightFlickerDistractionInvokable lightFlickerDistractionInvokable in lightFlickerDistractionInvokables)
                    {
                        lightFlickerDistractionInvokable.RevokeDistraction();
                    }
                    break;
                case 2:
                    radioDistractionInvokable.RevokeDistraction();
                    break;
                case 3:
                    nPCJumpscareDistractionInvokable.RevokeDistraction();
                    break;
                default:
                    Console.WriteLine("random distraction generated out of bounds");
                    break;
            }
        }
    }
}