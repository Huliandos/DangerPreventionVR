using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class InteractableObjectPatch : MonoBehaviour
{
    [HideInInspector] public bool objectIsGrabbed = false;
    [HideInInspector] public GameObject grabbingObject = null;
    
    VRTK_InteractableObject interactableObject;

    private SnapDropZonePatch currentlyReferencedDropZone;

    // Start is called before the first frame update
    void Start()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();

        interactableObject.InteractableObjectGrabbed += InteractableObjectGrabbed;
        interactableObject.InteractableObjectUngrabbed += InteractableObjectUngrabbed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CheckPossibleSnapping(SnapDropZonePatch dropZone)
    {
        if (currentlyReferencedDropZone == null)
        {
            currentlyReferencedDropZone = dropZone;
            return true;
        }

        Vector3 position = transform.position;
        float distanceCurrentDropZone = Vector3.Distance(position, currentlyReferencedDropZone.transform.position);
        float distanceNewDropZone = Vector3.Distance(position, dropZone.transform.position);
        
        if (distanceCurrentDropZone < distanceNewDropZone) 
            return false;

        currentlyReferencedDropZone = dropZone;
        return true;
    }

    void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e) {
        objectIsGrabbed = true;
        grabbingObject = e.interactingObject;

        Physics.IgnoreLayerCollision(1<<LayerMask.GetMask("InteractableObjects"), 1<<LayerMask.GetMask("PlayerHands"), true);
    }

    void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        objectIsGrabbed = false;
        grabbingObject = null;

        transform.parent = null;
        GetComponent<Rigidbody>().isKinematic = false;

        //foreach (Joint joint in GetComponents<Joint>()) Destroy(joint);

        Physics.IgnoreLayerCollision(1 << LayerMask.GetMask("InteractableObjects"), 1 << LayerMask.GetMask("PlayerHands"), false);
        //StartCoroutine(SetCollisionBack());
    }

    //IEnumerator SetCollisionBack()
    //{
    //    Debug.Log("Set back");
    //    yield return new WaitForSeconds(1f);
    //    Debug.Log("Stopped Waiting");
        
    //}

    public void ForceStopInteracting() {
        if(interactableObject == null) interactableObject = GetComponent<VRTK_InteractableObject>();

        //reset
        interactableObject.enabled = false;

        //parenting
        transform.parent = null;
        GetComponent<Rigidbody>().isKinematic = false;

        //fixed joint
        foreach (Joint joint in GetComponents<Joint>()) Destroy(joint);

        Physics.IgnoreLayerCollision(1 << LayerMask.GetMask("InteractableObjects"), 1 << LayerMask.GetMask("PlayerHands"), false);
    }

    public void setupNewGrabbingReference() {
        ForceStopInteracting();
        //find some way to setup the grabbing reference for the cut off child

        Physics.IgnoreLayerCollision(1 << LayerMask.GetMask("InteractableObjects"), 1 << LayerMask.GetMask("PlayerHands"), false);
    }
}
