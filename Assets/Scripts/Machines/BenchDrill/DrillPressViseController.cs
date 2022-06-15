using Sirenix.OdinInspector;
using UnityEngine;
using VRTK;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class DrillPressViseController : MonoBehaviour
{
    [Title("Crank")]
    [SceneObjectsOnly]
    public GameObject crank_Parent;

    [Title("Crank Counterpart")]
    [SceneObjectsOnly]
    public GameObject crankCounterpart_Parent;

    [Title("Workpiece")]
    [SceneObjectsOnly]
    public VRTK_SnapDropZone workpiece_dropZone;
    private VRTK_PolicyList policyList;
    public float fittingThreshold;

    [Title("Raycast Configuration")]
    public Transform raycastOriginPoint;
    public LayerMask layerMask;

    private RaycastHit raycastHit;
    private Vector3 direction = Vector3.back;
    private VRTK_ArtificialRotator crank_artRot;

    //public GameObject[] workpiecesInScene;

    private GameObject currentlyGrabbedObject;
    private SnapDropZonePatch dropZonePatch;

    private void Awake()
    {
        policyList = workpiece_dropZone.GetComponent<VRTK_PolicyList>();
        dropZonePatch = workpiece_dropZone.GetComponent<SnapDropZonePatch>();

        crank_artRot = (crank_artRot == null ? crank_Parent.GetComponent<VRTK_ArtificialRotator>() : crank_artRot);
        crank_artRot.ValueChanged += ValueChanged;

        //workpiece_dropZone.ObjectEnteredSnapDropZone += ObjectEnteredSnapDropZone;

        //foreach (GameObject workpiece in workpiecesInScene)
        //{
        //    if (workpiece.CompareTag(policyList.identifiers[0]))
        //    {
        //        workpiece.GetComponent<VRTK_InteractableObject>().InteractableObjectGrabbed += InteractableObjectGrabbed;
        //        workpiece.GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
        //    }
        //}

        workpiece_dropZone.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"other obj: {other.gameObject.name}");

        if (other.gameObject.CompareTag(policyList.identifiers[0]))
        {
            if (CheckIfObjectFits(other.gameObject))
            {
                EnableWorkpieceSnapDropZone();
            }
            else
            {
                if (workpiece_dropZone.GetComponent<SnapDropZonePatch>().SnappedObject == null) DisableWorkpieceSnapDropZone();
            }
        }
    }

    //private void ObjectEnteredSnapDropZone(object sender, SnapDropZoneEventArgs e)
    //{
    //    Debug.Log($"e obj = {e.snappedObject}");

    //    if (e.snappedObject != null && e.snappedObject.CompareTag(policyList.identifiers[0]))
    //    {
    //        if (CheckIfObjectFits(e.snappedObject))
    //        {
    //            EnableWorkpieceSnapDropZone();
    //        }
    //        else
    //        {
    //            DisableWorkpieceSnapDropZone();
    //        }
    //    }
    //}

    //private void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    //{
    //    currentlyGrabbedObject = e.interactingObject.GetComponent<VRTK_InteractGrab>().GetGrabbedObject().gameObject;
    //}


    //private void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    //{
    //    currentlyGrabbedObject = null;
    //}

    private bool CheckIfObjectFits(GameObject objectToCheck)
    {
        Physics.Raycast(raycastOriginPoint.position, direction, out raycastHit, 5f, layerMask);

        Vector3 raycastOrigin = raycastOriginPoint.position;
        Vector3 hitVector = raycastHit.point;

        Debug.Log($"Raycast Origin {raycastOrigin} , HitVector {hitVector}");

        float distance = Vector3.Distance(hitVector, raycastOrigin);

        Mesh mesh = objectToCheck.GetComponent<MeshFilter>().mesh;
        Bounds bounds = mesh.bounds;

        float objectWidth = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);

        Debug.Log($"Hit distance: {distance} , Object Width: {objectWidth} , fits: {distance >= (objectWidth + fittingThreshold)} ");

        if (distance >= (objectWidth + fittingThreshold))
        {
            return true;
            Debug.DrawRay(raycastOriginPoint.position, direction * raycastHit.distance, Color.green);
        }
        else
        {
            return false;
        }
    }

    private void EnableWorkpieceSnapDropZone()
    {
        if (!workpiece_dropZone.gameObject.activeSelf)
        {
            workpiece_dropZone.gameObject.SetActive(true);
        }
    }

    private void DisableWorkpieceSnapDropZone()
    {
        if (workpiece_dropZone.gameObject.activeSelf)
        {
            workpiece_dropZone.gameObject.SetActive(false);
        }
    }

    // could check if fits with currnomalval
    protected virtual void ValueChanged(object sender, ControllableEventArgs e)
    {
        float currNormVal = crank_artRot.GetNormalizedValue();

        crankCounterpart_Parent.GetComponent<Animator>().SetFloat("NormalizedDrillPressVisePosition", currNormVal);

        //if (currentlyGrabbedObject != null)
        //{
        //    if (CheckIfObjectFits(currentlyGrabbedObject))
        //    {
        //        EnableWorkpieceSnapDropZone();
        //    }
        //    else
        //    {
        //        DisableWorkpieceSnapDropZone();
        //    }
        //}

        //if (dropZonePatch.snappedObject != null)
        //{
        //    string log = "Curr: " + currNormVal + "\n";
        //    log += "Local scale " + dropZonePatch.snappedObject.transform.localScale.z;
        //    Debug.Log(log);

        //    if (currNormVal > dropZonePatch.snappedObject.transform.localScale.z)
        //    {
        //        dropZonePatch.snappedObject.GetComponent<VRTK_InteractableObject>().isGrabbable = true;
        //    }
        //    else
        //    {
        //        dropZonePatch.snappedObject.GetComponent<VRTK_InteractableObject>().isGrabbable = false;
        //    }
        //}
    }

    private void Update()
    {
        if (workpiece_dropZone.GetComponent<SnapDropZonePatch>().SnappedObject != null)
        {
            //Debug.Log($"object {workpiece_dropZone.GetComponent<SnapDropZonePatch>().SnappedObject} is snapped, not grabbable");
            gameObject.GetComponent<VRTK_InteractableObject>().isGrabbable = false;
        } else
        {
            //Debug.Log($"No object {workpiece_dropZone.GetComponent<SnapDropZonePatch>().SnappedObject} is snapped, grabbable");
            gameObject.GetComponent<VRTK_InteractableObject>().isGrabbable = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.DrawRay(raycastOriginPoint.position, direction * 0.5f, Color.red);
    }
}
