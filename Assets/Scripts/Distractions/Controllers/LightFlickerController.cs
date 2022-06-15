using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ToDo: Delete. Becomes obsolete once new system is reviewed
public class LightFlickerController : MonoBehaviour
{
    enum SearchTypes { THIS, ALL_CHILDREN, DIRECT_CHILDREN }

    [Header("Data fetch variables")]
    [SerializeField]
    SearchTypes searchForLightsIn = SearchTypes.THIS;

    [Header("Customization Variables")]
    [SerializeField] [Tooltip("The minimum amount of time (in seconds) the light stays in one state (on/off)")]
    float minLightStateFreezeDuration = 0;
    [SerializeField] [Tooltip("The maximum amount of time (in seconds) the light stays in one state (on/off)")]
    float maxLightStateFreezeDuration = 1;
    [SerializeField] [Tooltip("The maximum amount of times the lightstate is allowed to switch. Values under 0 mean, that lightflickering can only be stopped from calls on the StopFlickering method")]
    int maxLightstateSwitches = 15;

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
        else {
            List<Light> lightList = new List<Light>();

            lightList.AddRange(GetComponents<Light>());
            lightList.AddRange(transform.GetComponentsInDirectChildren<Light>());

            lights = lightList.ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) {
            StartFlickering();
        }
    }

    public void StartFlickering()
    {
        StopAllCoroutines();
        StartCoroutine(SwitchLightState(0));
    }

    IEnumerator SwitchLightState(int lightingSwitchCounter) {
        foreach (Light light in lights) { 
            light.enabled = (light.enabled == false ? true : false);
        }

        yield return new WaitForSeconds(Random.Range(Mathf.Clamp(minLightStateFreezeDuration, 0, maxLightStateFreezeDuration), maxLightStateFreezeDuration));   //wait a random amount of time between min/max light state freeze duration

        if (lightingSwitchCounter >= 0 && lightingSwitchCounter >= maxLightstateSwitches) StopFlickering();     //stop flickering after max num of flickers has been reached
        else StartCoroutine(SwitchLightState(lightingSwitchCounter+1));                                         //call switch light state again
    }

    public void StopFlickering() {
        StopAllCoroutines();

        foreach (Light light in lights) {
            light.enabled = true;
        }
    }
}
