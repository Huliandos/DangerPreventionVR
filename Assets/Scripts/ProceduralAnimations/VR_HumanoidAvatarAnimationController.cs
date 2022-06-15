using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VR_HumanoidAvatarAnimationController : MonoBehaviour
{
    #region serializedVariables
    [Header("Goal Targets for movement calculations")]
    [SerializeField]
    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    Transform[] legGoalTargets;

    [Header("VR Controllers/Headset following Variables")]
    [Tooltip("Transform Reference to the players VR eyes position")]
    Transform VReyes;

    [Tooltip("Transform Reference to the players VR controllers. In this order: L, R")]
    Transform[] VRcontrollers = new Transform[2];

    [Header("Avatar Bone references")]
    [SerializeField]
    [Tooltip("Transform Reference to the avatars eye position")]
    Transform eyes;

    [SerializeField]
    [Tooltip("Transform Reference to the avatars head")]
    Transform head;

    Transform neck;

    [SerializeField]
    [Tooltip("Transform Reference to the avatars hands. In this order: L, R")]
    Transform[] hands;

    [SerializeField]
    [Tooltip("Transform Reference to the avatars feet. In this order: L, R")]
    Transform[] feet;

    [Header("Variables to control lerping speed")]
    [SerializeField]
    [Range(0, 1)]
    float bodyHeightAdjustmentSpeed = .05f;
    float torsoNeckAdjutmentSpeed = .2f;

    [Header("References to IK Chain targets")]
    [SerializeField]
    [Tooltip("Targets for neck")]
    Transform neckTarget;

    [SerializeField]
    [Tooltip("Targets for each arm. In this order: L, R")]
    Transform[] handTargets;

    [SerializeField]
    [Tooltip("Targets for each leg. In this order: L, R")]
    Transform[] footTargets;
    #endregion

    #region IK rotation issue fix fields
    Vector3 armsLocalRotationAxis = new Vector3(0, 1, 0);
    Quaternion[] armsStartRots = new Quaternion[2];
    Quaternion[] legsStartRots = new Quaternion[2];

    Quaternion[] footTargetsStartRots = new Quaternion[2];
    #endregion

    #region gameLogicVariables
    [Header("Game logic variables")]
    bool[] legMoving;

    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    float legGoalTargetOffsetMagnitude;

    int numberOfLegs;

    [SerializeField]
    Vector3 footHighpointVector = new Vector3(0, 0.5f, 0);

    [SerializeField]
    float footBoneYoffset = 0.05f;

    Vector3 neckOffset, pelvisOffset;
    Vector3[] handsOffset = new Vector3[2];
    [SerializeField]
    Transform[] handTargetsOffset;
    Quaternion handTargetRotationOffset = Quaternion.Euler(0, 0, 180);

    Quaternion[] feetRotationOffset = new Quaternion[2];    //TODO!!

    //delete varibales below, once it's confirmed, that they aren't needed
    float bodyHeightOffset = .7f;
    #endregion

    void Start()
    {
        #region init leg offsets
        numberOfLegs = legGoalTargets.Length;

        legGoalTargetOffsetMagnitude = (transform.position - legGoalTargets[0].position - new Vector3(0, transform.position.y, 0)).magnitude;

        for (int i = 0; i < 2; i++)
        {
            feetRotationOffset[i] = feet[i].rotation;
        }

        /*
        legs = new Transform[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legs[i] = transform.GetComponentsInDirectChildren<Transform>()[i];
        }
        */
        #endregion

        neck = eyes.parent.parent;

        legMoving = new bool[numberOfLegs];
        neckOffset = eyes.InverseTransformDirection(neck.position - eyes.position);
        //neckOffset = head.position - neckTarget.position;
        pelvisOffset = transform.position - neck.position;
        handsOffset[0] = hands[0].InverseTransformDirection(hands[0].parent.position - hands[0].position);
        handsOffset[1] = hands[1].InverseTransformDirection(hands[1].parent.position - hands[1].position);
        //handsOffset[0] = handTargets[0].position - hands[0].position;
        //handsOffset[1] = handTargets[1].position - hands[1].position;

        armsStartRots[0] = hands[0].parent.parent.parent.localRotation;
        armsStartRots[1] = hands[1].parent.parent.parent.localRotation;
        legsStartRots[0] = feet[0].parent.parent.localRotation;
        legsStartRots[1] = feet[1].parent.parent.localRotation;
        footTargetsStartRots[0] = footTargets[0].rotation;
        footTargetsStartRots[1] = footTargets[1].rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //Weirdly enough, the VRTK References just get available inside the Update Cycle, not once the scene starts, so we have to fetch them here
        if (VReyes == null || VRcontrollers[0] == null || VRcontrollers[1] == null)
        {
            Transform VRTK_SDKManager = FindObjectOfType<VRTK_SDKManager>().transform;
            if(VRTK_SDKManager.GetComponentInChildren<Camera>()) VReyes = VRTK_SDKManager.GetComponentInChildren<Camera>().transform;
            foreach (VRTK_ControllerEvents controller in VRTK_SDKManager.GetComponentsInChildren<VRTK_ControllerEvents>())
            {
                if (controller.name.Contains("Left")) VRcontrollers[0] = controller.transform;
                else if (controller.name.Contains("Right")) VRcontrollers[1] = controller.transform;
            }
        }

        #region legMovement
        //goal Target position setting
        for (int i = 0; i < numberOfLegs; i++)
        {
            RaycastHit hit;
            if (i == 0)
            {//left leg
                Physics.Raycast(transform.position - transform.right*legGoalTargetOffsetMagnitude, -Vector3.up, out hit, 3, LayerMask.GetMask("Ground"));
                //Debug.DrawRay(transform.position - transform.right*legGoalTargetOffsetMagnitude, -Vector3.up, Color.red);
            }
            else
            { //right leg
                Physics.Raycast(transform.position + transform.right*legGoalTargetOffsetMagnitude, -Vector3.up, out hit, 3, LayerMask.GetMask("Ground"));
                //Debug.DrawRay(transform.position + transform.right*legGoalTargetOffsetMagnitude, -Vector3.up, Color.red);
            }

            if (hit.point == null)
            {
                //handling
            }
            else
            {
                legGoalTargets[i].position = hit.point;
            }
        }

        for (int i = 0; i < numberOfLegs; i++)
        {
            //if other leg ist moving
            foreach (bool isMoving in legMoving)
            {
                if (isMoving)
                    return;
            }

            //projecting point onto plane defined by objects forward axis and up axis as normal 
            Vector3 projectedLegGoalTarget = legGoalTargets[i].position - transform.position;   //Vector between point on plane and point to project
            float distance = Vector3.Dot(projectedLegGoalTarget, transform.up);
            projectedLegGoalTarget = projectedLegGoalTarget - transform.up * distance;

            Vector3 projectedLegTarget = footTargets[i].position - transform.position;   //Vector between point on plane and point to project
            distance = Vector3.Dot(projectedLegTarget, transform.up);
            projectedLegTarget = projectedLegTarget - transform.up * distance;

            //if distance between goal and target is too big
            if ((projectedLegGoalTarget - projectedLegTarget).magnitude > .5f)
            {
                legMoving[i] = true;
                StartCoroutine(smoothMoveLegTargets(footTargets[i], legGoalTargets[i].position, i));
            }
        }
        #endregion

        followVRPeripherie();

        if (VRcontrollers[0] != null)
        {
            hands[0].position = handTargetsOffset[0].position;
            hands[0].rotation = handTargetsOffset[0].rotation;
        }
        if (VRcontrollers[1] != null)
        {
            hands[1].position = handTargetsOffset[1].position;
            hands[1].rotation = handTargetsOffset[1].rotation;
        }

        adjustArmsPosition();


        //adjust upper arm bone rotation
        for (int i = 0; i < hands.Length; i++)
        {
            Transform upperArm = hands[i].parent.parent.parent;    //very hardcoded TODO: change later

            Quaternion change = Quaternion.AngleAxis(transform.parent.eulerAngles.z, armsLocalRotationAxis);
            upperArm.localRotation = armsStartRots[i] * change;

            hands[i].parent.GetComponent<InverseKinematics_AdjustableRootrot>().RotatedRootBone();
        }


        //adjust upper leg bone rotation
        for (int i = 0; i < feet.Length; i++)
        {
            Transform thigh = feet[i].parent.parent;    //very hardcoded TODO: change later

            Quaternion change = Quaternion.AngleAxis(transform.parent.eulerAngles.z, Vector3.up);
            thigh.localRotation = legsStartRots[i] * change;

            feet[i].GetComponent<InverseKinematics_AdjustableRootrot>().RotatedRootBone();
        }
    }


    private void FixedUpdate()
    {
        //followVRPeripherie();
    }
    

    void followVRPeripherie() {
        //Consider lerping here
        if (VReyes != null)
        {
            #region Bending back
            /*
            neckTarget.position = Vector3.Lerp(transform.position, head.position + neckOffset, bodyHeightAdjustmentSpeed);

            float x, z;
            head.rotation = VReyes.rotation;

            //Go through all bones till pelvis
            x = neckTarget.localEulerAngles.x;
            z = neckTarget.localEulerAngles.z;

            neckTarget.rotation = Quaternion.Lerp(neckTarget.rotation, head.rotation, bodyHeightAdjustmentSpeed);
            neckTarget.localEulerAngles = new Vector3(x, neckTarget.localEulerAngles.y, z);
            */
            #endregion

            #region Bending knees 
            Vector3 neckPosition = VReyes.position + VReyes.TransformDirection(neckOffset);

            //eyes.position = VReyes.position;

            //neck.position = eyes.position + eyes.TransformDirection(neckOffset);
            //neck.rotation = Quaternion.LookRotation(VReyes.forward, eyes.position - neck.position);

            //transform.position = neck.position + pelvisOffset;
            transform.position = neckPosition + pelvisOffset;

            neck.position = neckPosition;
            neck.rotation = Quaternion.LookRotation(VReyes.forward, VReyes.position - neck.position);

            eyes.position = VReyes.position;

            float x, z;
            eyes.rotation = VReyes.rotation;

            #region whole rig
            x = transform.parent.localEulerAngles.x;
            z = transform.parent.localEulerAngles.z;

            //transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, eyes.rotation, bodyHeightAdjustmentSpeed);
            transform.parent.rotation = eyes.rotation;
            transform.parent.localEulerAngles = new Vector3(x, transform.parent.localEulerAngles.y, z);
            #endregion

            #endregion
        }
    }

    void adjustNeckPosition()
    {
        neckTarget.position = Vector3.Lerp(neckTarget.position, eyes.position + neckOffset, torsoNeckAdjutmentSpeed);
    }

    void adjustArmsPosition()
    {
        for (int i = 0; i < handTargets.Length; i++) {
            handTargets[i].position = hands[i].position; // + hands[i].TransformDirection(handsOffset[i])
            handTargets[i].rotation = hands[i].rotation;
        }
    }

    IEnumerator smoothMoveLegTargets(Transform legTarget, Vector3 legGoalTarget, int arrayPos)
    {
        float t = 0;
        Vector3 legTargetStartingPos = legTarget.position;

        legGoalTarget = new Vector3(legGoalTarget.x, legGoalTarget.y + footBoneYoffset, legGoalTarget.z);

        while (t <= 1)
        {
            t += .2f;
            t = Mathf.Round(t * 10) / 10;   //to solve rounding error where t gets incremented by more than .1f

            //raises and lowers the leg linearly
            //ToDo? Change movement to parabola

            //move position
            if (t <= .5f)
                legTarget.position = Vector3.Lerp(legTargetStartingPos, legGoalTarget + footHighpointVector, t);
            else
                legTarget.position = Vector3.Lerp(legTargetStartingPos + footHighpointVector, legGoalTarget, t);

            //yield return new WaitForSeconds(Time.deltaTime);
            yield return null;
        }
        legMoving[arrayPos] = false;

        yield return null;
    }
}
