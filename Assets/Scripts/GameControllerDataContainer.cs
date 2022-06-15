using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GameControllerDataContainer : MonoBehaviour
{
    [Header("Avatar Parts")]
    [Tooltip("[0] - avatar_final_female, [1] - RightHand_Female, [2] - LeftHand_Female")]
    [SerializeField] List<GameObject> avatarParts_female = new List<GameObject>();
    [Tooltip("[0] - avatar_final_male, [1] - RightHand_Male, [2] - LeftHand_Male")]
    [SerializeField] List<GameObject> avatarParts_male = new List<GameObject>();

    [SerializeField] SwitchVRControllersOnStartup switchVRControllers;
    [SerializeField] VRTK_HeadsetFade headsetFade;
    [SerializeField] GameObject vrtkSetup;
    [SerializeField] Transform headsetCameraRig;

    /// <summary>
    /// Getter for the list of GameObjects belonging to the female avatar (rig, right/left hand)
    /// </summary>
    /// <returns> The list of GameObjects belonging to the female avatar (rig, right/left hand) </returns>
    public List<GameObject> ReturnFemaleAvatarParts()
    {
        return avatarParts_female;
    }

    /// <summary>
    /// Getter for the list of GameObjects belonging to the male avatar (rig, right/left hand)
    /// </summary>
    /// <returns> The list of GameObjects belonging to the male avatar (rig, right/left hand) </returns>
    public List<GameObject> ReturnMaleAvatarParts()
    {
        return avatarParts_male;
    }

    /// <summary>
    /// Getter for the SwitchVRControllersOnStartup component on the RightControllerAlias scene object
    /// </summary>
    /// <returns> The SwitchVRControllersOnStartup component on the RightControllerAlias scene object </returns>
    public SwitchVRControllersOnStartup ReturnSwitchVRControllersScript()
    {
        return switchVRControllers;
    }

    /// <summary>
    /// Getter for VRTK_HeadsetFade component on player camera. Starts coroutine to find it
    /// </summary>
    /// <returns> The reference to the VRTK_HeadsetFade component on the camera </returns>
    public VRTK_HeadsetFade ReturnHeadsetFade()
    {
        StartCoroutine(FindHMD(() =>
        {
            if (headsetFade == null) headsetFade = VRTK_DeviceFinder.HeadsetCamera().GetComponent<VRTK_HeadsetFade>();
        }));
        
        return headsetFade;
    }

    /// <summary>
    /// Getter for VRTK_CameraRig Transform to influence rotation of player camera.
    /// </summary>
    /// <returns>The reference to the VRTK_CameraRig Transform in the scene</returns>
    public Transform ReturnHeadsetCameraRig()
    {
        StartCoroutine(FindHMD(() =>
        {
            if (headsetCameraRig == null) headsetCameraRig = VRTK_DeviceFinder.HeadsetCamera().parent.parent;
        }));
        
        return headsetCameraRig;
    }

    /// <summary>
    /// Getter for the VRTK_Setup scene object
    /// </summary>
    /// <returns> The VRTK_Setup scene object </returns>
    public GameObject ReturnVRTKSetup()
    {
        return vrtkSetup;
    }

    /// <summary>
    /// Due to VRTK needing some time to enable the headset at start, this coroutine checks again for it after some time
    /// </summary>
    /// <returns> The reference to the VRTK_HeadsetFade component on the camera </returns>
    IEnumerator FindHMD(Action action = null)
    {
        while (VRTK_DeviceFinder.HeadsetCamera() == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"Found {VRTK_DeviceFinder.HeadsetCamera()}");
        action?.Invoke();
    }
}