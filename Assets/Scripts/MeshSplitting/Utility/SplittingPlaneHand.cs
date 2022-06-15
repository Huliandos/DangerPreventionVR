using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SplittingPlaneHand : MonoBehaviour
{
    /// <summary>
    /// Class that is used as a means to communicate between the cutting algorithms and the scene
    /// It fetches the values we need from objects in Range and parses them into A Cutter object, so that it calculates the cut
    /// </summary>

    //First list holds all objects within range, the second one holds the indices of all objects that have left the range
    List<GameObject> collidingSplittables = new List<GameObject>();
    List<int> destroyedSplittablesIndeces = new List<int>();

    //Reference to the script calculating full engulfment
    CheckCompleteEngulfment engulfmentChecker;

    [SerializeField]
    [Tooltip("Parent object of this GameObject. Is used as visual Feedback for the player")]
    Renderer splittingPlaneVisualFeedbackRenderer;

    [SerializeField]
    [Tooltip("Sets the number of Raycasts for engulfment checking. The more rays the heavier on perfomrance, but the smaller objects can be noticed within range")]
    int numberOfRaycasts = 50;

    //Set to the four corners of our visual Feedback plane
    Vector3[] raycastPoints;

    //flag that gets set to true once the destroyedSplittablesIndeces is populted, so collidingSplittables can be cleared in the next Update Step
    bool clearList;

    // Start is called before the first frame update
    void Start()
    {
        engulfmentChecker = GetComponent<CheckCompleteEngulfment>();

        //translation: {bottom left, top right, bottom right, top left} of our visible splitting plane
        raycastPoints = new Vector3[4] { new Vector3(-splittingPlaneVisualFeedbackRenderer.transform.localScale.x/2, 0, -splittingPlaneVisualFeedbackRenderer.transform.localScale.z/2),
            new Vector3(splittingPlaneVisualFeedbackRenderer.transform.localScale.x/2, 0, splittingPlaneVisualFeedbackRenderer.transform.localScale.z/2),
            new Vector3(splittingPlaneVisualFeedbackRenderer.transform.localScale.x/2, 0, -splittingPlaneVisualFeedbackRenderer.transform.localScale.z/2),
            new Vector3(-splittingPlaneVisualFeedbackRenderer.transform.localScale.x/2, 0, splittingPlaneVisualFeedbackRenderer.transform.localScale.x/2)};
    }

    // Update is called once per frame
    void Update()
    {
        if (clearList)
        {
            for (int i = destroyedSplittablesIndeces.Count - 1; i > -1; i--)
                collidingSplittables.RemoveAt(destroyedSplittablesIndeces[i]);

            clearList = false;
            destroyedSplittablesIndeces = new List<int>();
        }
    }

    public void MultiThreadCut()
    {
        //Thread thread = new Thread(() => cut());
        //thread.Start();

        Cut();
    }

    public void Cut()
    {
        List<GameObject> cutObjects = new List<GameObject>();
        for (int i = 0; i < collidingSplittables.Count; i++)
        {
            //All lost and old references to Objects get cued up to be deleted out of our collidingObjects List in the next frame
            if (collidingSplittables[i] == null)
            {
                clearList = true;
                destroyedSplittablesIndeces.Add(i);
            }
            else
            {
                if (collidingSplittables[i].GetComponent<RootObject>())
                { //if we colide with a child of a compound cillider

                    // change sound if cutting hands
                    if (transform.GetComponentInParent<HandLeverShearsController>())
                    {
                        Transform leverShearTransform = transform.GetComponentInParent<HandLeverShearsController>().transform;
                        leverShearTransform.GetComponent<AudioSource>().clip = leverShearTransform.GetComponent<HandLeverShearsController>().handCutClip;
                    }

                    GameObject goToCut;
                    //goToCut = collidingSplittables[i].GetComponent<RootObject>().getRoot();
                    goToCut = collidingSplittables[i].GetComponent<RootObject>().getMeshRenderer().gameObject;

                    Vector3 collisionPoint = GetCollisionPoint(collidingSplittables[i]);
                    
                    GetComponent<SpawnBlood>().Bleed(collisionPoint, collisionPoint - collidingSplittables[i].GetComponent<RootObject>().getMeshRenderer().transform.position);
                    //GetComponent<SpawnBlood>().Bleed(collisionPoint, collisionPoint - collidingSplittables[i].GetComponent<RootObject>().getRoot().GetComponentInChildren<SkinnedMeshRenderer>().transform.position);

                    if (!cutObjects.Contains(goToCut))
                    {
                        Cutter cutter = new Cutter();
                        if (goToCut.GetComponent<SplittableInfo>())
                        { //if Object includes SplittableInfo script then use its information to initialize the cut

                            cutter.InitCutOnOtherThread(goToCut, GetCollisionPoint(goToCut, collidingSplittables[i]), transform.up
                                , goToCut.GetComponent<SplittableInfo>().getCutMaterial(), goToCut.GetComponent<SplittableInfo>().getCap()
                                , goToCut.GetComponent<SplittableInfo>().getConcave(), goToCut.GetComponent<SplittableInfo>().getMeshSeperation(), true);

                            /*
                            cutter.cut(goToCut, getCollisionPoint(goToCut, collidingSplittables[i]), transform.up
                                , goToCut.GetComponent<SplittableInfo>().getCutMaterial(), goToCut.GetComponent<SplittableInfo>().getCap()
                                , goToCut.GetComponent<SplittableInfo>().getConcave(), goToCut.GetComponent<SplittableInfo>().getMeshSeperation(), true);
                            */
                        }
                        else
                        {//if not then use default values
                            cutter.InitCutOnOtherThread(goToCut, GetCollisionPoint(goToCut, collidingSplittables[i]), transform.up, null, false, false, false, true);

                            //cutter.cut(goToCut, getCollisionPoint(goToCut, collidingSplittables[i]), transform.up, null, false, false, false, true);
                        }

                        cutObjects.Add(goToCut);
                    }
                }
                else
                {  //default cut behavior
                    if (collidingSplittables[i].GetComponent<IsFlesh>() != null)
                    {
                        Vector3 collisionPoint = GetCollisionPoint(collidingSplittables[i]);
                        GetComponent<SpawnBlood>().Bleed(collisionPoint, collisionPoint - collidingSplittables[i].transform.position);
                    }

                    if (collidingSplittables[i].GetComponent<IsFlesh>() != null)
                    {
                        Vector3 collisionPoint = GetCollisionPoint(collidingSplittables[i]);
                        GetComponent<SpawnBlood>().Bleed(collisionPoint, collisionPoint - collidingSplittables[i].transform.position);
                    }

                    Cutter cutter = new Cutter();
                    if (collidingSplittables[i].GetComponent<SplittableInfo>())
                    { //if Object includes SplittableInfo script then use its information to initialize the cut
                      // Debug.Log("Computed contact point" + getCollisionPoint(collidingSplittables[i]));
                        Debug.Log(GetCollisionPoint(collidingSplittables[i]));

                        cutter.InitCutOnOtherThread(collidingSplittables[i], GetCollisionPoint(collidingSplittables[i]), transform.up
                                , collidingSplittables[i].GetComponent<SplittableInfo>().getCutMaterial(), collidingSplittables[i].GetComponent<SplittableInfo>().getCap()
                                , collidingSplittables[i].GetComponent<SplittableInfo>().getConcave(), collidingSplittables[i].GetComponent<SplittableInfo>().getMeshSeperation(), false);

                        /*
                        cutter.cut(collidingSplittables[i], getCollisionPoint(collidingSplittables[i]), transform.up
                                , collidingSplittables[i].GetComponent<SplittableInfo>().getCutMaterial(), collidingSplittables[i].GetComponent<SplittableInfo>().getCap()
                                , collidingSplittables[i].GetComponent<SplittableInfo>().getConcave(), collidingSplittables[i].GetComponent<SplittableInfo>().getMeshSeperation(), false);
                        */
                    }
                    else  //if not then use default values
                    {
                        cutter.InitCutOnOtherThread(collidingSplittables[i], GetCollisionPoint(collidingSplittables[i]), transform.up);

                        //cutter.cut(collidingSplittables[i], getCollisionPoint(collidingSplittables[i]), transform.up);
                    }
                }

                clearList = true;
                destroyedSplittablesIndeces.Add(i);
            }
        }
    }

    //Method used in the Mechanic test scene. Called once the Meshes have been swapped
    public void ClearCollidingSplittables()
    {
        collidingSplittables.Clear();
    }

    //this is crude. find a way to optimize it
    Vector3 GetCollisionPoint(GameObject toFind, GameObject actualCollidingChild = null)
    {
        Collider col;
        if (actualCollidingChild == null) col = toFind.GetComponent<Collider>();
        else col = actualCollidingChild.GetComponent<Collider>();

        //Renderer rend = transform.parent.GetComponent<Renderer>();
        //Debug.Log(rend.bounds.extents.z);

        RaycastHit hit;
        int numOfRaycasts = 20;
        for (int i = 0; i <= numOfRaycasts; i++)
        {
            //move i to fit the objects z extents
            float divider = (float)numOfRaycasts / transform.parent.localScale.x;

            //Debug.Log(transform.parent.localScale.x);

            //Raycast along the Z axis
            Vector3 startingPoint = new Vector3(i / divider - transform.parent.localScale.x / 2, transform.position.y, transform.position.z);
            transform.TransformPoint(startingPoint);
            Ray ray = new Ray(startingPoint, transform.TransformDirection(transform.forward));
            col.Raycast(ray, out hit, transform.parent.localScale.z / 2);


            if (hit.collider != null)
            {
                return hit.point;
            }

            ray = new Ray(startingPoint, -transform.TransformDirection(transform.forward));
            col.Raycast(ray, out hit, transform.parent.localScale.z / 2);

            //Debug.Log("GetCollisionPoint starting point z: " + startingPoint);

            if (hit.collider != null)
            {
                return hit.point;
            }


            //move i to fit the objects X extents
            divider = (float)numOfRaycasts / transform.parent.localScale.z;

            //Raycast along the X axis
            startingPoint = new Vector3(transform.position.x, transform.position.y, i / divider - transform.parent.localScale.z / 2);
            transform.TransformPoint(startingPoint);
            ray = new Ray(startingPoint, transform.TransformDirection(transform.right));
            col.Raycast(ray, out hit, transform.parent.localScale.z / 2);

            if (hit.collider != null)
            {
                return hit.point;
            }

            ray = new Ray(startingPoint, -transform.TransformDirection(transform.right));
            col.Raycast(ray, out hit, transform.parent.localScale.x / 2);

            //Debug.Log("GetCollisionPoint starting point x: " + startingPoint);

            if (hit.collider != null)
            {
                return hit.point;
            }
        }

        //Object to cut is in the middle of this object
        //return transform.position - toFind.transform.position
        return transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        //if object isn't yet considered to be colliding
        if (!collidingSplittables.Contains(other.gameObject) && other.gameObject.tag == Tags.splittable)
        {
            //to evade reference type errors
            //Vector3[] copiedRaycastPoints = new Vector3[4];
            //raycastPoints.CopyTo(copiedRaycastPoints, 0);

            //if the Object is enngulfed by our visual feedback plane then add it to our collidingSplittables List and give the player visual confirmation
            //if (engulfmentChecker.isEngulfedInCollider(other.GetComponent<MeshCollider>(), copiedRaycastPoints, numberOfRaycasts)) {
            collidingSplittables.Add(other.gameObject);

            splittingPlaneVisualFeedbackRenderer.material.color = Color.red;
            //}
        }
        //if object is considered to be colliding
        else if (collidingSplittables.Contains(other.gameObject) && other.gameObject.tag == Tags.splittable)
        {
            //Vector3[] copiedRaycastPoints = new Vector3[4];
            //raycastPoints.CopyTo(copiedRaycastPoints, 0);

            //if the Object isn't enngulfed anymore by our visual feedback plane then remove it to our collidingSplittables List and give the player visual confirmation if there's no more colliding objects
            //if (!engulfmentChecker.isEngulfedInCollider(other.GetComponent<MeshCollider>(), copiedRaycastPoints, numberOfRaycasts))
            //{
            //collidingSplittables.Remove(other.gameObject);

            if (collidingSplittables.Count == 0)
                splittingPlaneVisualFeedbackRenderer.material.color = new Color(1, 1, 1, 20 / 255);
            //}
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //safety function so that object are really removed once they Exit the collider, in case OnTriggerStay didn't catch them leave it
        if (collidingSplittables.Contains(other.gameObject) && other.gameObject.tag == Tags.splittable)
        {
            collidingSplittables.Remove(other.gameObject);

            if (collidingSplittables.Count == 0)
                splittingPlaneVisualFeedbackRenderer.material.color = new Color(1, 1, 1, 20 / 255);
        }
    }

    private void OnEnable()
    {
        splittingPlaneVisualFeedbackRenderer.material.color = new Color(1, 1, 1, 20 / 255);
    }

    ///alternative to always become cuttable after collision.
    ///Exchange this with the OnTriggerStayMethod
    ///Doesn't check for complete engulfment
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (!collidingSplittables.Contains(other.gameObject) && other.gameObject.tag == Tags.splittalbe){
            collidingSplittables.Add(other.gameObject);
        
            splittingPlaneVisualFeedbackRenderer.material.color = Color.red;    
        }
    }
    */
}
