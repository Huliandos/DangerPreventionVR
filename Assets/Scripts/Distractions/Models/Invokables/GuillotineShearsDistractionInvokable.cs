using UnityEngine;
using VRTK.Controllables.ArtificialBased;

namespace Distractions.Models.Invokables
{
    public class GuillotineShearsDistractionInvokable : DistractionInvokable
    {
        [SerializeField]
        AudioClip breakNoise;
        [SerializeField]
        ParticleSystem smoke, sparks;

        GuillotineShearsController guillotineShearsController;

        private void Start()
        {
            guillotineShearsController = GetComponent<GuillotineShearsController>();
        }

        public override void InvokeDistraction<T>(T distractionData)
        {
            base.InvokeDistraction(distractionData);

            VRTK_ArtificialPusher aP = guillotineShearsController.startknopf_Parent.GetComponent<VRTK_ArtificialPusher>();

            // TODO: make green button not interactable anymore
            guillotineShearsController.machineIsBroken = true;
            aP.pressedDistance = 0;

            // TODO: play broken sound
            guillotineShearsController.GetComponentInChildren<AudioSource>().PlayOneShot(breakNoise);

            // TODO: play particle system
            smoke.Play();
            sparks.Play();
        }

        public override void RevokeDistraction()
        {
            base.RevokeDistraction();

            //Do nothing here, as the distraction cancels itself after its done playing
        }
    }
}