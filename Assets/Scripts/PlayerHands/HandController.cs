using UnityEngine;
using VRTK;

public class HandController : MonoBehaviour
{
    #region private fields

    Animator animator;
    string thumb = "Thumb", index = "Index", middle = "Middle", ring = "Ring", pinky = "Pinky";
    VRTK_ControllerEvents controllerEvents;
    VRTK_TrackedController trackedController;

    #endregion

    #region serialized fields

    [SerializeField] bool leftController;

    #endregion

    void Start()
    {
        //Getting references
        animator = GetComponent<Animator>();

        if (leftController)
            controllerEvents = GameObject.Find("LeftControllerScriptAlias").GetComponent<VRTK_ControllerEvents>();
        else
            controllerEvents = GameObject.Find("RightControllerScriptAlias").GetComponent<VRTK_ControllerEvents>();

        trackedController = controllerEvents.transform.parent.GetComponent<VRTK_TrackedController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controllerEvents.touchpadPressed)
            animator.SetFloat(thumb, Mathf.Lerp(animator.GetFloat(thumb), 1, .2f));
        else
            animator.SetFloat(thumb, Mathf.Lerp(animator.GetFloat(thumb), 0, .2f));

        animator.SetFloat(index, controllerEvents.GetTriggerAxis());

        animator.SetFloat(middle, controllerEvents.GetTriggerAxis());
        animator.SetFloat(ring, controllerEvents.GetTriggerAxis());
        animator.SetFloat(pinky, controllerEvents.GetTriggerAxis());

        //if (controllerEvents.gripPressed)
        //{
        //    animator.SetFloat(middle, Mathf.Lerp(animator.GetFloat(middle), 1, .2f));
        //    animator.SetFloat(ring, Mathf.Lerp(animator.GetFloat(ring), 1, .2f));
        //    animator.SetFloat(pinky, Mathf.Lerp(animator.GetFloat(pinky), 1, .2f));
        //}
        //else
        //{
        //    animator.SetFloat(middle, Mathf.Lerp(animator.GetFloat(middle), 0, .2f));
        //    animator.SetFloat(ring, Mathf.Lerp(animator.GetFloat(ring), 0, .2f));
        //    animator.SetFloat(pinky, Mathf.Lerp(animator.GetFloat(pinky), 0, .2f));
        //}



        /* ToDo:
         * this doesn't work yet. Idk why. Resolve in the future.
         * Idea: Move each finger seperately according to its position on the controller
         * Problem: Sense Axis doesn't return any value but 0
         * For Index VR Controllers
        animator.SetFloat(thumb, controllerEvents.GetTouchpadSenseAxis());
        animator.SetFloat(index, controllerEvents.GetTriggerSenseAxis());
        animator.SetFloat(middle, controllerEvents.GetMiddleFingerSenseAxis());
        animator.SetFloat(ring, controllerEvents.GetRingFingerSenseAxis());
        animator.SetFloat(pinky, controllerEvents.GetPinkyFingerSenseAxis());
        */

        //Implement this, once the problem above is solved
        //if (trackedController.GetControllerType() == SDK_BaseController.ControllerType.SteamVR_ValveKnuckles)
        //{ //Move each Finger Seperately

        //}
        //else /*if (trackedController.GetControllerType() == SDK_BaseController.ControllerType.SteamVR_ViveWand)*/ //Add other controller schemes for different controllers
        //{ //Move each Finger Seperately
        //if (controllerEvents.touchpadPressed)
        //    animator.SetFloat(thumb, Mathf.Lerp(animator.GetFloat(thumb), 1, .2f));
        //else
        //    animator.SetFloat(thumb, Mathf.Lerp(animator.GetFloat(thumb), 0, .2f));

        //animator.SetFloat(index, controllerEvents.GetTriggerAxis());

        //if (controllerEvents.gripPressed)
        //{
        //    animator.SetFloat(middle, Mathf.Lerp(animator.GetFloat(middle), 1, .2f));
        //    animator.SetFloat(ring, Mathf.Lerp(animator.GetFloat(ring), 1, .2f));
        //    animator.SetFloat(pinky, Mathf.Lerp(animator.GetFloat(pinky), 1, .2f));
        //}
        //else
        //{
        //    animator.SetFloat(middle, Mathf.Lerp(animator.GetFloat(middle), 0, .2f));
        //    animator.SetFloat(ring, Mathf.Lerp(animator.GetFloat(ring), 0, .2f));
        //    animator.SetFloat(pinky, Mathf.Lerp(animator.GetFloat(pinky), 0, .2f));
        //}
        //}
    }
}
