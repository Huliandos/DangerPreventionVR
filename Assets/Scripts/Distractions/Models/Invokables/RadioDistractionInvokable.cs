using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Distractions.Models.Invokables
{
    public class RadioDistractionInvokable : DistractionInvokable
    {
        [SerializeField]
        AudioClip[] noise, newsFlash;

        RadioController radioController;

        private void Start()
        {
            radioController = GetComponent<RadioController>();
        }

        public void ChooseRandomRadioDistraction(bool silence = false)
        {
            if (!silence)
            {
                int randomNum = Random.Range(0, 3); //random between 0 and 2
                AudioClip randomClip = null;

                //noise
                if (randomNum == 0)
                {
                    randomClip = noise[Random.Range(0, noise.Length)];
                    radioController.SwitchSong(randomClip);
                }
                //newsFlash
                else if (randomNum == 1)
                {
                    randomClip = newsFlash[Random.Range(0, newsFlash.Length)];
                    radioController.SwitchSong(randomClip);
                }
                else
                {
                    radioController.SilenceRadio();
                }

                return;
            }
            radioController.SilenceRadio();
        }

        public override void InvokeDistraction<T>(T distractionData)
        {
            base.InvokeDistraction(distractionData);

            ChooseRandomRadioDistraction();
        }

        public override void RevokeDistraction()
        {
            base.RevokeDistraction();

            //Do nothing here, as the distraction cancels itself after its done playing
            //radioController.switchSong();
        }
    }
}
