using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_HumanoidAvatarAnimationController : MonoBehaviour
{
    #region serializedVariables
    [Header("Goal Targets for movement calculations")]
    [SerializeField]
    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    Transform[] legGoalTargets;

    [Header("VR Controllers/Headset following Variables")]
    [Tooltip("Transform Reference to the players VR eyes position")]
    [SerializeField]    //remove Serialize Field once testing is done
    Transform VReyes;

    [Header("Avatar Bone references")]
    [SerializeField]
    [Tooltip("Transform Reference to the players head")]
    Transform neck;

    [SerializeField]
    [Tooltip("Transform Reference to the players head. In this order: L, R")]
    Transform[] hands;

    [SerializeField]
    [Tooltip("Transform Reference to the players head. In this order: L, R")]
    Transform[] feet;

    [Header("Variables to control lerping speed")]
    [SerializeField]
    [Range(0, 1)]
    float bodyFollowSpeed = .05f;

    [SerializeField]
    float torsoNeckAdjutmentSpeed = .2f;

    [SerializeField]
    float jumpscareMovementSpeed = .1f;

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

    #region gameLogicVariables
    [Header("Game logic variables")]
    [SerializeField]
    AudioClip[] oneLiners;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Transform metarigRotationReference;

    bool[] legMoving;

    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    float legGoalTargetOffsetMagnitude;

    int numberOfLegs;

    [SerializeField]
    Vector3 footHighpointVector = new Vector3(0, 0.5f, 0);

    [SerializeField]
    float footBoneYoffset = 0.05f;

    Vector3[] handsOffset = new Vector3[2];
    Quaternion[] feetRotationOffset = new Quaternion[2];    //TODO!!

    enum AI_Behaviours { WALK_TO_DESTINATION,  JUMPSCARE_PLAYER, WAIT}

    [SerializeField]
    AI_Behaviours AI_Behaviour = AI_Behaviours.WAIT;
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

        legMoving = new bool[numberOfLegs];
        handsOffset[0] = handTargets[0].position - hands[0].position;
        handsOffset[1] = handTargets[1].position - hands[1].position;
    }

    // Update is called once per frame
    void Update()
    {
        //Weirdly enough, the VRTK References just get available inside the Update Cycle, not once the scene starts, so we have to fetch them here
        if (VReyes == null)
        {
            Transform VRTK_SDKManager = FindObjectOfType<VRTK.VRTK_SDKManager>().transform;
            if (VRTK_SDKManager.GetComponentInChildren<Camera>()) VReyes = VRTK_SDKManager.GetComponentInChildren<Camera>().transform;
        }

        #region legMovement
        //goal Target position setting
        for (int i = 0; i < numberOfLegs; i++)
        {
            RaycastHit hit;
            if (i == 0)
            {//left leg
                Physics.Raycast(transform.position - transform.right * legGoalTargetOffsetMagnitude, -Vector3.up, out hit, 3, LayerMask.GetMask("Ground"));
                Debug.DrawRay(transform.position - transform.right * legGoalTargetOffsetMagnitude, -Vector3.up, Color.red);
            }
            else
            { //right leg
                Physics.Raycast(transform.position + transform.right * legGoalTargetOffsetMagnitude, -Vector3.up, out hit, 3, LayerMask.GetMask("Ground"));
                Debug.DrawRay(transform.position + transform.right * legGoalTargetOffsetMagnitude, -Vector3.up, Color.red);
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
            if ((projectedLegGoalTarget - projectedLegTarget).magnitude > .4f)
            {
                legMoving[i] = true;
                StartCoroutine(smoothMoveLegTargets(footTargets[i], legGoalTargets[i].position, i));
            }
        }
        #endregion
    }

    private void FixedUpdate()
    {
        #region AI Behaviour Tree
        switch (AI_Behaviour) {
            case AI_Behaviours.WALK_TO_DESTINATION:
                Debug.Log(AI_Behaviour + " Behaviour not implemented");

                AI_Behaviour = AI_Behaviours.WAIT;
                break;
            case AI_Behaviours.JUMPSCARE_PLAYER:
                Debug.Log("Starting Jumpscare");
                if (VReyes != null)
                {
                    InitiatePlayerJumpscare();

                    AI_Behaviour = AI_Behaviours.WAIT;
                }
                break;
            default:
                //Debug.Log("Awaiting state change");
                break;
        }

        #endregion

        BodyFollowNeck();
        //adjustBodyHeight();
        //adjustNeckPosition();
        //adjustArmsPosition();
    }

    void InitiatePlayerJumpscare()
    {
        //Start IEnumerator to raise arm
        StartCoroutine(WaveHands());

        //Optional: Curl Fingers

        //Lean into Camera
        StartCoroutine(LeanIntoCamera());

        //Drop voice line

        //Walk back to door/despawn

    }

    /// <summary>
    /// Moves the Avatars Hands into wave position, waves them around for num of "waveIterations" and then resets them next to the avatars body
    /// </summary>
    /// <param name="animSpeed">The speed of all animations in this animation set. Ranges from 0 (no movement) to 1. Lowest value possible is .01f</param>
    /// <param name="waveIterations">How often the hands should  move left to right when waving</param>
    /// <returns></returns>
    IEnumerator WaveHands(float animSpeed = .075f, int waveIterations = 15) {
        //define start and goal positions for hands in local coords
        Vector3 waveStartPosR = new Vector3(.25f, 1.7f, .2f);
        Quaternion waveStartRotR = Quaternion.Euler(18, 265, -335);
        Vector3 waveGoalPosR = new Vector3(.5f, 1.7f, .2f);
        Quaternion waveGoalRotR = Quaternion.Euler(-13, 265, -335);

        Vector3 waveStartPosL = new Vector3(-.25f, 1.7f, .2f);
        Quaternion waveStartRotL = Quaternion.Euler(18, 75, -8);
        Vector3 waveGoalPosL = new Vector3(-.5f, 1.7f, .2f);
        Quaternion waveGoalRotL = Quaternion.Euler(-13, 75, -8);

        Vector3 resetGoalPosR = new Vector3(.4f, .95f, .085f);
        Quaternion resetGoalRotR = Quaternion.Euler(-25, 2.5f, -160);

        Vector3 resetGoalPosL = new Vector3(-.4f, .95f, .085f);
        Quaternion resetGoalRotL = Quaternion.Euler(-10, 75, -230);

        Transform parent = transform.parent;

        bool reverse = false;

        //move hands to start position
        for (float t = 0f; t <= 1; t+=animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            //left hand
            handTargets[0].localPosition = Vector3.Lerp(handTargets[0].localPosition, metarigRotationReference.TransformDirection(waveStartPosL)+transform.parent.localPosition, t);
            //handTargets[0].localRotation = Quaternion.Lerp(handTargets[0].localRotation, waveStartRotL, t);
            handTargets[0].LookAt(VReyes);

            //right hand
            handTargets[1].localPosition = Vector3.Lerp(handTargets[1].localPosition, metarigRotationReference.TransformDirection(waveStartPosR) + transform.parent.localPosition, t);
            //handTargets[1].localRotation = Quaternion.Lerp(handTargets[1].localRotation, waveStartRotR, t);
            handTargets[1].LookAt(VReyes);

            yield return 0;
        }

        //wave hands
        for (int iterations = 0; iterations < waveIterations; iterations++)
        {
            if (iterations == 5)
            {
                audioSource.clip = oneLiners[Random.Range(0, oneLiners.Length)];
                audioSource.Play();
            }

            for (float t = 0f; t <= 1; t += animSpeed)
            {
                t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

                // ToDo: Waving doesn't work properly yet
                if (!reverse)
                {
                    //left hand
                    handTargets[0].localPosition = metarigRotationReference.TransformDirection(Vector3.Lerp(waveStartPosL, waveGoalPosL, t)) + parent.localPosition;
                    //handTargets[0].localRotation = Quaternion.Lerp(waveStartRotL, waveGoalRotL, t);
                    handTargets[0].LookAt(VReyes);

                    //right hand
                    handTargets[1].localPosition = metarigRotationReference.TransformDirection(Vector3.Lerp(waveStartPosR, waveGoalPosR, t)) + parent.localPosition;
                    //handTargets[1].localRotation = Quaternion.Lerp(waveStartRotR, waveGoalRotR, t);
                    handTargets[1].LookAt(VReyes);
                }
                else
                {
                    //left hand
                    handTargets[0].localPosition = metarigRotationReference.TransformDirection(Vector3.Lerp(waveGoalPosL, waveStartPosL, t)) + parent.localPosition;
                    //handTargets[0].localRotation = Quaternion.Lerp(waveGoalRotL, waveStartRotL, t);
                    handTargets[0].LookAt(VReyes);

                    //right hand
                    handTargets[1].localPosition = metarigRotationReference.TransformDirection(Vector3.Lerp(waveGoalPosR, waveStartPosR, t)) + parent.localPosition;
                    //handTargets[1].localRotation = Quaternion.Lerp(waveGoalRotR, waveStartRotR, t);
                    handTargets[1].LookAt(VReyes);
                }

                yield return 0;
            }
            reverse = reverse == true ? false : true;
        }

        //reset hands to reset position
        for (float t = 0f; t <= 1; t += animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            //left hand
            handTargets[0].localPosition = Vector3.Lerp(handTargets[0].localPosition, metarigRotationReference.TransformDirection(resetGoalPosL) + transform.parent.localPosition, t);
            //handTargets[0].localRotation = Quaternion.Lerp(handTargets[0].localRotation, resetGoalRotL, t);
            handTargets[0].LookAt(VReyes);

            //right hand
            handTargets[1].localPosition = Vector3.Lerp(handTargets[1].localPosition, metarigRotationReference.TransformDirection(resetGoalPosR) + transform.parent.localPosition, t);
            //handTargets[1].localRotation = Quaternion.Lerp(handTargets[1].localRotation, resetGoalRotR, t);
            handTargets[1].LookAt(VReyes);

            yield return 0;
        }
    }

    IEnumerator LeanIntoCamera(float animSpeed = .025f)
    {
        int randomSideModifier = 1;
        if (Random.Range(0, 2) == 0) {  //50% chance to randomly go to players left
            randomSideModifier = -1;
        }
        

        //define start and goal positions for hands in local coords
        Vector3 neckStartPos = neckTarget.position;
        Vector3 neckGoalPos = VReyes.transform.position + VReyes.transform.forward*.6f + VReyes.transform.right*.4f*randomSideModifier;
        Vector3 neckMidwayPos = (VReyes.transform.position + VReyes.transform.forward*.6f/2 + VReyes.transform.right * .4f * randomSideModifier*2+transform.position)/2;

        //to not move neck up and down
        neckGoalPos = new Vector3(neckGoalPos.x, neckStartPos.y, neckGoalPos.z);
        neckMidwayPos = new Vector3(neckMidwayPos.x, neckStartPos.y, neckMidwayPos.z);

        //move neck to midway pos
        for (float t = 0f; t <= 1; t += animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            neckTarget.position = Vector3.Lerp(neckStartPos, neckMidwayPos, t);
            neckTarget.LookAt(VReyes);

            yield return 0;
        }
        //move neck to goal pos
        for (float t = 0f; t <= 1; t += animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            neckTarget.position = Vector3.Lerp(neckMidwayPos, neckGoalPos, t);
            neckTarget.LookAt(VReyes);

            yield return 0;
        }

        StartCoroutine(LeanOutOfCamera(neckMidwayPos, animSpeed));
    }
    IEnumerator LeanOutOfCamera(Vector3 neckMidwayPos, float animSpeed = .025f)
    {
        yield return new WaitForSeconds(2);
        //define start and goal positions for neck in coords
        Vector3 neckStartPos = neckTarget.position;
        Vector3 neckGoalPos = VReyes.transform.position - VReyes.transform.forward * .6f;

        //to not move neck up and down
        neckGoalPos = new Vector3(neckGoalPos.x, neckStartPos.y, neckGoalPos.z);

        //move neck to midway pos
        for (float t = 0f; t <= 1; t += animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            neckTarget.position = Vector3.Lerp(neckStartPos, neckMidwayPos, t);
            neckTarget.LookAt(VReyes);

            yield return 0;
        }
        //move hands to start position
        for (float t = 0f; t <= 1; t += animSpeed)
        {
            t = Mathf.Round(t * 100) / 100;   //to solve rounding error where t gets incremented by more than .1f

            neckTarget.position = Vector3.Lerp(neckMidwayPos, neckGoalPos, t);
            neckTarget.LookAt(VReyes);

            yield return 0;
        }

        Destroy(transform.root.gameObject);
    }

    void BodyFollowNeck() {
        transform.parent.position = Vector3.Lerp(transform.parent.position, neckTarget.position, jumpscareMovementSpeed);
        transform.parent.position = new Vector3(transform.parent.position.x, 0, transform.parent.position.z);

        float x, y, z;
        /*
        //pelvis
        x = transform.localEulerAngles.x;
        z = transform.localEulerAngles.z;

        transform.rotation = Quaternion.Lerp(transform.rotation, neckTarget.rotation, bodyHeightAdjustmentSpeed);
        transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, z);
        */

        //whole rig so targets can keep on using local position
        x = transform.parent.localEulerAngles.x;
        z = transform.parent.localEulerAngles.z;

        transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, neckTarget.rotation, bodyFollowSpeed);
        transform.parent.localEulerAngles = new Vector3(x, transform.parent.localEulerAngles.y, z);
        

        #region adjust IK Root Bones rotation
        //adjust stomach rotation
        Transform stomach = neck.parent.parent;    //very hardcoded TODO: change later

        x = stomach.localEulerAngles.x;
        z = stomach.localEulerAngles.z;

        stomach.rotation = transform.rotation; //Quaternion.Lerp(thigh.rotation, head.rotation, bodyHeightAdjustmentSpeed);
        stomach.localEulerAngles = new Vector3(x, stomach.localEulerAngles.y, z);

        neck.GetComponent<InverseKinematics_AdjustableRootrot>().RotatedRootBone();
        
        //adjust Shoulder bone rotation
        for (int i = 0; i < hands.Length; i++)
        {
            //hands is convenience bone!
            Transform shoulder = hands[i].parent.parent.parent;    //very hardcoded TODO: change later

            x = shoulder.localEulerAngles.x;
            z = shoulder.localEulerAngles.z;

            //shoulder.rotation = transform.rotation;
            shoulder.LookAt(VReyes, Vector3.up);
            shoulder.localEulerAngles = new Vector3(x, shoulder.localEulerAngles.y, z);

            //if (i==0) Debug.Log("Left: " + thigh.eulerAngles);
            //else Debug.Log("Right: " + thigh.eulerAngles);

            hands[i].parent.GetComponent<InverseKinematics_AdjustableRootrot>().RotatedRootBone();
        }

        //adjust upper leg bone rotation
        for (int i = 0; i < feet.Length; i++)
        {
            Transform thigh = feet[i].parent.parent;    //very hardcoded TODO: change later

            x = thigh.localEulerAngles.x;
            z = thigh.localEulerAngles.z;

            thigh.rotation = transform.rotation; //Quaternion.Lerp(thigh.rotation, head.rotation, bodyHeightAdjustmentSpeed);
            thigh.localEulerAngles = new Vector3(x, thigh.localEulerAngles.y, z);

            //if (i==0) Debug.Log("Left: " + thigh.eulerAngles);
            //else Debug.Log("Right: " + thigh.eulerAngles);

            feet[i].GetComponent<InverseKinematics_AdjustableRootrot>().RotatedRootBone();
        }

        //adjust foot rotation to match hips

        for (int i = 0; i < footTargets.Length; i++)
        {
            x = footTargets[i].localEulerAngles.x;
            y = footTargets[i].localEulerAngles.y;

            footTargets[i].rotation = transform.rotation;
            footTargets[i].localEulerAngles = new Vector3(x, y, footTargets[i].localEulerAngles.z);
        }
        #endregion
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

            yield return new WaitForSeconds(Time.deltaTime);
        }
        legMoving[arrayPos] = false;

        yield return null;
    }
}
