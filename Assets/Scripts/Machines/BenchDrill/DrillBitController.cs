using Sirenix.OdinInspector;
using UnityEngine;
using VRTK;

public class DrillBitController : MonoBehaviour
{
    public enum DrillBitState
    {
        NOT_SNAPPED,
        IMPROPERLY_SNAPPED,
        PROPERLY_SNAPPED
    }

    [SerializeField]
    public DrillBitState State { get; private set; }

    #region private fields

    [SerializeField] private BenchDrillController bd_controller;
    [SerializeField] private DrillChuckController dc_controller;
    private GameController gameController;
    private PlayerController playerController;
    private bool drillBitLaunched = false;

    #endregion

    #region serialized fields

    [SerializeField] Transform drillBitImpactPosition;
    [SerializeField] float invokeTime = 1f;
    [SerializeField] float appliedForce = 1f;

    #endregion

    [Title("Debug Shortcut")]
    [Button(ButtonSizes.Large)]
    private void Launch()
    {
        Invoke("LaunchDrillBit", invokeTime);
    }

    private void Awake()
    {
        gameController = GameController.Instance;
        playerController = FindObjectOfType<PlayerController>();

        State = DrillBitState.NOT_SNAPPED;
    }

    private void Update()
    {

        Debug.Log($"is drilling? {bd_controller.drilling} , same obj? {dc_controller.db_sdzPatch.SnappedObject}");
        if (dc_controller.db_sdzPatch.SnappedObject != gameObject || !bd_controller.drilling)
        {
            transform.GetComponentInDirectChildren<Collider>().enabled = false;
            transform.GetComponentInDirectChildren<ApplyDamageOnCollision>().ClearCollidingObjects();
        }
        else if (bd_controller.drilling)
        {
            transform.GetComponentInDirectChildren<Collider>().enabled = true;

            //if (State == DrillBitState.IMPROPERLY_SNAPPED && !drillBitLaunched)
            //{
            //    drillBitLaunched = true;
            //    Invoke("LaunchDrillBit", invokeTime);
            //}
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SnappedBehaviour()
    {
        if (dc_controller.db_sdzPatch.SnappedObject == gameObject)
        {
            if (dc_controller.State == DrillChuckController.DrillChuckState.FULLY_OPENED)
            {
                //State = DrillBitState.NOT_SNAPPED;
                //ResetObjectProperties();
                State = DrillBitState.IMPROPERLY_SNAPPED;
            }
            else if (dc_controller.State == DrillChuckController.DrillChuckState.PARTIALLY_OPENED)
            {
                State = DrillBitState.IMPROPERLY_SNAPPED;
                ToggleGrabbable(true);
            }
            else if (dc_controller.State == DrillChuckController.DrillChuckState.FULLY_CLOSED)
            {
                State = DrillBitState.PROPERLY_SNAPPED;
                ToggleGrabbable(false);
            }

            Debug.Log("dc: " + dc_controller.State + " || db: " + State);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void UpdateSnappedBehaviour(DrillChuckController.DrillChuckState state)
    {
        if (dc_controller.db_sdzPatch.SnappedObject == gameObject)
        {
            switch (state)
            {
                case DrillChuckController.DrillChuckState.FULLY_OPENED:
                    //State = DrillBitState.NOT_SNAPPED;
                    //if (!gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed()) ResetObjectProperties();
                    State = DrillBitState.IMPROPERLY_SNAPPED;
                    break;
                case DrillChuckController.DrillChuckState.PARTIALLY_OPENED:
                    State = DrillBitState.IMPROPERLY_SNAPPED;
                    ToggleGrabbable(true);
                    break;
                case DrillChuckController.DrillChuckState.FULLY_CLOSED:
                    State = DrillBitState.PROPERLY_SNAPPED;
                    ToggleGrabbable(false);
                    break;
                default:
                    break;
            }
        }
        Debug.Log("UPDATED dc: " + state + " || db: " + State);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isGrabbable"></param>
    private void ToggleGrabbable(bool isGrabbable)
    {
        gameObject.GetComponent<VRTK_InteractableObject>().isGrabbable = isGrabbable;
    }

    public void LaunchDrillBit()
    {
        ResetObjectProperties();
        Vector3 vector = CalculateVectorToPlayer(gameObject.transform.position);
        gameObject.GetComponent<Rigidbody>().AddForce(vector * appliedForce, ForceMode.Impulse);
        Invoke("SetImpactPosition", 0.2f);

    }

    private Vector3 CalculateVectorToPlayer(Vector3 drillBitpos)
    {
        Vector3 playerPos = drillBitImpactPosition.position;
        Vector3 forceDirection = playerPos - drillBitpos;
        return forceDirection;
    }

    private void SetImpactPosition()
    {
        gameObject.transform.parent = drillBitImpactPosition;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(180, 0, 0);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        if (playerController.SafetyGlassesAreOn)
        {
            // play glass impact sound
            playerController.StartCoroutine("PlayEyeHitWithGlassesSound");

            // game over with minor inj clipboard
            gameController.GameOver(Injury.Safety);
        }
        else
        {
            // play flesh impact sound
            playerController.StartCoroutine("PlayEyeHitWithoutGlassesSound");

            // show red ignette vfx
            playerController.DisplayInjuryVFX(true);

            // game over with major inj clipboard
            gameController.GameOver(Injury.Headshot);
        }
    }

    public void ResetObjectProperties()
    {
        dc_controller.db_sdzPatch.ResetSnappedObject();
        transform.GetComponentInDirectChildren<Collider>().enabled = false;
        gameObject.transform.SetParent(null);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO: just triggering with pointy part of db, not with the side going into the chuck because there isn't a trigger collider there
        if (State == DrillBitState.NOT_SNAPPED && other.gameObject == dc_controller.gameObject.GetComponentInChildren<BoxCollider>().gameObject && bd_controller.drilling)
        {
            Debug.Log("Hit drill chuck while drilling and not being snapped");
        }
    }
}
