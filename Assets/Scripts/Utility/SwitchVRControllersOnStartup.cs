using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SwitchVRControllersOnStartup : MonoBehaviour
{
    VRTK_ControllerEvents controllerEvents;

    int frameCounter;

    // Start is called before the first frame update
    void Start()
    {
        controllerEvents = GetComponent<VRTK_ControllerEvents>();
    }

    void Update()
    {
        frameCounter++;

        //As per usual, VRTK scripts aren't initialized the very first frame
        //Thus we have to wait a few frames, before our controller input gets properly recognized
        //Tried moving this into a Coroutine, but weirdly enough that didn't work.
        //This solution isn't optimal, but its good enough for now
        if (frameCounter == 12)
        {
            controllerEvents.OnButtonTwoPressed(controllerEvents.SetControllerEvent(ref controllerEvents.buttonTwoPressed, true, 1f));
            Destroy(this);
        }
    }
}
