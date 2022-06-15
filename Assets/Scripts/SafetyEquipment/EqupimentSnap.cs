namespace M7.Scripts.Mechanics
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector;
    using VRTK;
    using UnityEngine.Events;
    using System;

    public class EqupimentSnap : MonoBehaviour
    {
        [SerializeField]
        VRTK_InteractableObject snapPrefab;

        [SerializeField]
        VRTK_SnapDropZone snapZone;

        [SerializeField]
        Transform parentTransform;

        [SerializeField]
        // AudioMuffling audioMuffling;

        private bool isInSnapZone = false;
        private bool isSnapped = false;

        private bool objectAlreadySnapped = false;

        private void Awake()
        {
            snapZone.snapType = VRTK_SnapDropZone.SnapTypes.UseKinematic;

            snapZone.ObjectEnteredSnapDropZone += new SnapDropZoneEventHandler(Set_IsInSnapZone);
            snapZone.ObjectExitedSnapDropZone += new SnapDropZoneEventHandler(Unset_IsInSnapZone);

            snapPrefab.InteractableObjectGrabbed += new InteractableObjectEventHandler(Grab);
            snapPrefab.InteractableObjectUngrabbed += new InteractableObjectEventHandler(Ungrab);
        }

        private void Set_IsInSnapZone(object sender, SnapDropZoneEventArgs e)
        {
            isInSnapZone = true;
        }

        private void Unset_IsInSnapZone(object sender, SnapDropZoneEventArgs e)
        {
            isInSnapZone = false;
        }

        //e is the interacting object, aka the controller
        private void Grab(object sender, InteractableObjectEventArgs e)
        {
            GameObject grabbedObject = e.interactingObject.GetComponent<VRTK_InteractGrab>().GetGrabbedObject().gameObject;
            VRTK_PolicyList snapZonePolicyList = this.gameObject.GetComponent<VRTK_PolicyList>();
            VRTK_SnapDropZone snapDropZone = this.gameObject.GetComponent<VRTK_SnapDropZone>();

            if (isInSnapZone && grabbedObject.CompareTag(snapZonePolicyList.identifiers[0]))
            {
                snapDropZone.ForceUnsnap();
                grabbedObject.transform.SetParent(null);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;

                if (grabbedObject.CompareTag("Earmuffs"))
                {
                    Debug.Log("Normal sound playing...");
                    // audioMuffling.ResetSound();
                }

                Debug.Log("Unsnapped Object: " + grabbedObject.name + " || GRAVITY : " + grabbedObject.GetComponent<Rigidbody>().useGravity + " || KINEMATIC : " + grabbedObject.GetComponent<Rigidbody>().isKinematic);
            }
        }

        private void Ungrab(object sender, InteractableObjectEventArgs e)
        {
            GameObject grabbedObject = e.interactingObject.GetComponent<VRTK_InteractGrab>().GetGrabbedObject().gameObject;
            VRTK_PolicyList snapZonePolicyList = snapZone.gameObject.GetComponent<VRTK_PolicyList>();
            VRTK_SnapDropZone snapDropZone = this.gameObject.GetComponent<VRTK_SnapDropZone>();

            // TODO: iterate through policy list
            if (isInSnapZone && grabbedObject.CompareTag(snapZonePolicyList.identifiers[0]))
            {
                snapDropZone.ForceSnap(grabbedObject);
                grabbedObject.transform.SetParent(snapDropZone.gameObject.transform, false);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.GetComponent<Rigidbody>().useGravity = false;

                if (grabbedObject.CompareTag("Earmuffs"))
                {
                    Debug.Log("Muffled sound playing...");
                    // audioMuffling.MuffleSound();
                }

                Debug.Log("Snapped Object: " + grabbedObject.name + " || GRAVITY : " + grabbedObject.GetComponent<Rigidbody>().useGravity + " || KINEMATIC : " + grabbedObject.GetComponent<Rigidbody>().isKinematic);
            }

            else if (!isInSnapZone)
            {
                grabbedObject.transform.SetParent(null);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
}
