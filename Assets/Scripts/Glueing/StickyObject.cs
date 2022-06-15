using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class StickyObject : MonoBehaviour
{
    #region private fields

    //float additionalColliderSize = .15f;
    List<GameObject> gameObjectsSubscribedTo = new List<GameObject>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;

        //Bounds bounds = GetComponent<Renderer>().bounds;
        Bounds bounds = GetComponent<MeshFilter>().mesh.bounds;

        float xScale = Mathf.Abs(bounds.center.x - bounds.max.x) * 2;
        float yScale = Mathf.Abs(bounds.center.y - bounds.max.y) * 2;
        float zScale = Mathf.Abs(bounds.center.z - bounds.max.z) * 2;

        //ToDo: Change this to not be done hard coded, but to acutally use the bounding box to calculate the whole trigger collider
        //This works fine for cutting workpieces scaled (.75, .025, ,75)
        Vector3 colSize = new Vector3((xScale * 1.33f), (yScale * 4f), (zScale * 1.33f));

        col.size = colSize;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == Tags.splittable)
        {
            //don't add an object, that's already sticking to this object
            if ((other.GetComponent<FixedJoint>() && other.GetComponent<FixedJoint>().connectedBody == gameObject.GetComponent<Rigidbody>()) ||
                (GetComponent<FixedJoint>() && GetComponent<FixedJoint>().connectedBody == other.GetComponent<Rigidbody>())) { }
            else
            {
                other.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
                gameObjectsSubscribedTo.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == Tags.splittable)
        {
            other.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed -= InteractableObjectUngrabbed;
        }
    }

    void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        //if object is released in Trigger Range, parent it so it sticks to this Object. Also Increase this Objects Collider.
        GameObject objectToGlue = e.interactingObject.GetComponent<VRTK_InteractGrab>().GetGrabbedObject().gameObject;

        objectToGlue.AddComponent<FixedJoint>().connectedBody = GetComponent<Rigidbody>();

        //remove Sticky property from this object. Remove Trigger Collider
        foreach (Collider col in GetComponents<Collider>())
        {
            if (col.isTrigger)
            {
                Destroy(col);
                break;
            }
        }

        foreach (GameObject go in gameObjectsSubscribedTo)
        {
            go.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed -= InteractableObjectUngrabbed;
        }

        Destroy(this);
    }
}
