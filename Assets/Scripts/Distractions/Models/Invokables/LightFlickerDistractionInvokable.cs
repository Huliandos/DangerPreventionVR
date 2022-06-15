using Distractions.Management.EventSystem.DataContainer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Distractions.Models.Invokables
{
    public class LightFlickerDistractionInvokable : DistractionInvokable
    {
        enum SearchTypes { THIS, ALL_CHILDREN, DIRECT_CHILDREN }

        [Header("Data fetch variables")]
        [SerializeField]
        SearchTypes searchForLightsIn = SearchTypes.THIS;

        [Header("Customization Variables")]
        [SerializeField]
        [Tooltip("The minimum amount of time (in seconds) the light stays in one state (on/off)")]
        float minLightStateFreezeDuration = 0;
        [SerializeField]
        [Tooltip("The maximum amount of time (in seconds) the light stays in one state (on/off)")]
        float maxLightStateFreezeDuration = 1;
        [SerializeField]
        [Tooltip("The maximum amount of times the lightstate is allowed to switch. Values under 0 mean, that lightflickering can only be stopped from calls on the StopFlickering method")]
        int maxLightstateSwitches = 15;

        bool ongoingFlickering = false;

        Light[] lights;

        // Start is called before the first frame update
        void Start()
        {
            if (searchForLightsIn == SearchTypes.THIS)
            {
                lights = GetComponents<Light>();
            }
            else if (searchForLightsIn == SearchTypes.ALL_CHILDREN)
            {
                lights = GetComponentsInChildren<Light>();
            }
            else
            {
                List<Light> lightList = new List<Light>();

                lightList.AddRange(GetComponents<Light>());
                lightList.AddRange(transform.GetComponentsInDirectChildren<Light>());

                lights = lightList.ToArray();
            }
        }

        public void StartFlickering()
        {
            if (!ongoingFlickering)
            {
                //only flicker initially turned on lights
                ArrayList enabledLights = new ArrayList();
                foreach (Light light in lights)
                {
                    if (light.isActiveAndEnabled == true && light.gameObject.activeSelf == true) enabledLights.Add(light);
                }

                StopFlickering(enabledLights);

                ongoingFlickering = true;

                StartCoroutine(SwitchLightState(0, enabledLights));
            }
        }

        IEnumerator SwitchLightState(int lightingSwitchCounter, ArrayList lights)
        {
            foreach (Light light in lights)
            {
                light.enabled = (light.enabled == false ? true : false);
            }

            yield return new WaitForSeconds(Random.Range(Mathf.Clamp(minLightStateFreezeDuration, 0, maxLightStateFreezeDuration), maxLightStateFreezeDuration));   //wait a random amount of time between min/max light state freeze duration

            if (lightingSwitchCounter >= 0 && lightingSwitchCounter >= maxLightstateSwitches) StopFlickering(lights);     //stop flickering after max num of flickers has been reached
            else StartCoroutine(SwitchLightState(lightingSwitchCounter + 1, lights));                                         //call switch light state again
        }

        public void StopFlickering(ArrayList lights)
        {
            ongoingFlickering = false;

            StopAllCoroutines();

            foreach (Light light in lights)
            {
                light.enabled = true;
            }
        }

        public override void InvokeDistraction<T>(T distractionData)
        {
            base.InvokeDistraction(distractionData);

            StartFlickering();
        }

        public override void RevokeDistraction()
        {
            base.RevokeDistraction();

            //Do nothing here. Destroying it feels odd gameflow wise
            //StopFlickering();
        }
    }
}