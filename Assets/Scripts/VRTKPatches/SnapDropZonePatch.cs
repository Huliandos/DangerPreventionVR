using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SnapDropZonePatch : MonoBehaviour
{
    #region private fields

    private PlayerController playerController;
    private VRTK_SnapDropZone dropZone;
    private VRTK_PolicyList policyList;
    private GameObject grabbedObject;
    private Quaternion originalObjectYRotation;

    #endregion

    #region public fields

    public bool attachSnappedObjectToDropZone = true;
    [HideInInspector] public bool isInSnapZone = false;
    [HideInInspector] public bool objectWasSnapped = false;
    [HideInInspector] public GameObject SnappedObject { get; private set; }
    [HideInInspector]
    public Quaternion OriginalObjectYRotation
    {
        get => originalObjectYRotation;
        set
        {
            Vector3 rotation = transform.rotation.eulerAngles;
            originalObjectYRotation = Quaternion.Euler(rotation.x, value.eulerAngles.y, rotation.z);
        }
    }

    #endregion

    [SerializeField]
    bool forceToLocalOrigin;

    public event Action onObjectSuccesfullySnapped;
    public event Action onObjectSuccesfullyUnsnapped;

    private Dictionary<GameObject, bool> ungrabEventSubs = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        dropZone = gameObject.GetComponent<VRTK_SnapDropZone>();
        policyList = gameObject.GetComponent<VRTK_PolicyList>();
        dropZone.ObjectEnteredSnapDropZone += ObjectEnteredSnapDropZone;
        dropZone.ObjectExitedSnapDropZone += ObjectExitedSnapDropZone;

        playerController = FindObjectOfType<PlayerController>();
    }

    private void ObjectEnteredSnapDropZone(object sender, SnapDropZoneEventArgs e)
    {
        Debug.Log($"{e.snappedObject.name} entered {this.gameObject.name}");

        isInSnapZone = true;
        GameObject go = e.snappedObject;
        
        if (ungrabEventSubs.ContainsKey(go))
        {
            if (!ungrabEventSubs[go])
            {
                ungrabEventSubs[go] = true;
                e.snappedObject.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
            }
        }
        else
        {
            e.snappedObject.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
            ungrabEventSubs.Add(go, true);
        }
    }

    //private void Update()
    //{
    //    Debug.Log("Snapped Object: " + SnappedObject);
    //}

    private void ObjectExitedSnapDropZone(object sender, SnapDropZoneEventArgs e)
    {
        //TODO: earmuffs need to stop muffling audio on sdz exit not on unsnap
        //Hacky
        Debug.Log($"{e.snappedObject.name} exited {this.gameObject.name}");
        isInSnapZone = false;
        //TODO: test this stuff
        //if  (grabbedObject.activeSelf && grabbedObject.name == "Earmuffs") SetPlayerControllerValues("Earmuffs outside", false);
    }

    private void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        grabbedObject = e.interactingObject.GetComponent<VRTK_InteractGrab>().GetGrabbedObject().gameObject;
        InteractableObjectPatch interactingObjectPatch = grabbedObject.GetComponent<InteractableObjectPatch>();

        if (gameObject.activeSelf)
        {
            if (isInSnapZone && grabbedObject.CompareTag(policyList.identifiers[0]) && interactingObjectPatch.CheckPossibleSnapping(this))
            {
                if (grabbedObject.GetComponent<Utility.MoveGOPivot>()) grabbedObject.GetComponent<Utility.MoveGOPivot>().MovePivotToCenter();
                if (attachSnappedObjectToDropZone) grabbedObject.transform.SetParent(dropZone.gameObject.transform, false);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.GetComponent<Rigidbody>().useGravity = false;

                objectWasSnapped = true;
                SnappedObject = grabbedObject;
                OriginalObjectYRotation = SnappedObject.transform.rotation;

                SetPlayerControllerValues(SnappedObject.tag, true);

                ObjectSuccessfullySnapped();
            }
            else if (!isInSnapZone)
            {
                grabbedObject.transform.SetParent(null);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;

                objectWasSnapped = false;
                SnappedObject = null;

                SetPlayerControllerValues(grabbedObject.tag, false);
                
                ObjectSuccessfullyUnsnapped();
            }
        }

        if (ungrabEventSubs.ContainsKey(grabbedObject))
        {
            if (ungrabEventSubs[grabbedObject])
            {
                ungrabEventSubs[grabbedObject] = false;
                grabbedObject.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed -= InteractableObjectUngrabbed;
            }
        }

    }

    private void ObjectSuccessfullySnapped() => onObjectSuccesfullySnapped?.Invoke();

    private void ObjectSuccessfullyUnsnapped() => onObjectSuccesfullyUnsnapped?.Invoke();

    public void ResetSnappedObject()
    {
        if (SnappedObject != null) SnappedObject = null;
    }

    // Outsource to inheriting class EquipmentSnap : Monobehaviour, SnapDropZonePatch
    private void SetPlayerControllerValues(string snappedObjectTag, bool equipmentWasSnapped)
    {
        switch (snappedObjectTag)
        {
            case "Glasses":
                playerController.SafetyGlassesAreOn = equipmentWasSnapped;
                Debug.Log("Glasses are on: " + playerController.SafetyGlassesAreOn);
                break;
            case "Earmuffs":
                playerController.EarmuffsAreOn = equipmentWasSnapped;
                Debug.Log("Earmuffs are on: " + playerController.EarmuffsAreOn);
                break;
            case "Earmuffs outside":
                playerController.EarmuffsAreOn = equipmentWasSnapped;
                Debug.Log("Earmuffs are outside sdz");
                break;
            case "Helmet":
                playerController.HelmetIsOn = equipmentWasSnapped;
                Debug.Log("Helmet is on: " + playerController.HelmetIsOn);
                break;
            default:
                break;
        }
    }
}
