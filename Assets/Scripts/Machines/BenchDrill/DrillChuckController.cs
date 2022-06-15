using System.Collections;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;

public class DrillChuckController : MonoBehaviour
{
    public enum DrillChuckState
    {
        FULLY_OPENED,
        PARTIALLY_OPENED,
        FULLY_CLOSED
    }

    public DrillChuckState State { get; private set; }
    public GameObject dc;
    public VRTK_SnapDropZone db_sdz;
    public SnapDropZonePatch db_sdzPatch { get; private set; }

    private BenchDrillController bd_controller;
    private DrillBitController[] db_controllers;
    [SerializeField] private AudioClip openedClip;
    [SerializeField] private AudioClip closedClip;

    private VRTK_RotateTransformGrabAttach dc_Parent_rotTransGrabAttach;

    private void Awake()
    {
        dc_Parent_rotTransGrabAttach = (dc_Parent_rotTransGrabAttach == null ? gameObject.GetComponent<VRTK_RotateTransformGrabAttach>() : dc_Parent_rotTransGrabAttach);
        dc_Parent_rotTransGrabAttach.AngleChanged += AngleChanged;
        dc_Parent_rotTransGrabAttach.MaxAngleReached += MaxAngleReached;
        dc_Parent_rotTransGrabAttach.MinAngleReached += MinAngleReached;

        bd_controller = FindObjectOfType<BenchDrillController>();
        db_controllers = FindObjectsOfType<DrillBitController>();
        db_sdzPatch = db_sdz.GetComponent<SnapDropZonePatch>();
    }

    // Start is called before the first frame update
    void Start()
    {
        State = DrillChuckState.PARTIALLY_OPENED;

        foreach (DrillBitController db_controller in db_controllers)
        {
            db_sdzPatch.onObjectSuccesfullySnapped -= db_controller.SnappedBehaviour;
            db_sdzPatch.onObjectSuccesfullySnapped += db_controller.SnappedBehaviour;
        }

        if (db_sdz != null) db_sdz.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (bd_controller.drilling && db_sdzPatch.SnappedObject == null) db_sdz.gameObject.SetActive(false);
        else if (!bd_controller.drilling && State == DrillChuckState.FULLY_OPENED) db_sdz.gameObject.SetActive(true);

        //if (gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed() && bd_controller.drilling)
        //{
        //    TODO: this should happen when touching the dc, not neccesarily with grabbing
        //    Debug.Log("Injury");
        //    TODO: spawn blood here
        //    //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(1);
        //}
    }

    /// <summary>
    /// Indicates that the drill chuck is being rotated.
    /// <seealso cref="https://vrtoolkit.readme.io/docs/vrtk_rotatetransformgrabattach"/>
    /// </summary>
    /// <param name="sender"> This object </param>
    /// <param name="e"> Event payload <see cref="RotateTransformGrabAttachEventArgs"/> </param>
    private void AngleChanged(object sender, RotateTransformGrabAttachEventArgs e)
    {
        float currNormAngle = e.normalizedAngle;
        float minMaxNormTreshold = dc_Parent_rotTransGrabAttach.minMaxNormalizedThreshold;

        // OPENING: Normalized angle in between of: [0.5 -> 1f (Max)]
        if (currNormAngle >= 0.5f && (currNormAngle < (1f - minMaxNormTreshold)))
        {
            State = DrillChuckState.PARTIALLY_OPENED;

            UpdateStates(DrillBitController.DrillBitState.IMPROPERLY_SNAPPED, DrillChuckState.PARTIALLY_OPENED);
        }
        // CLOSING: Normalized angle in between of: [(Min) 0f <- 0.5]
        else if (currNormAngle < 0.5f && (currNormAngle > (0f + minMaxNormTreshold)))
        {
            State = DrillChuckState.PARTIALLY_OPENED;
            //TODO: if db is not ungrabbed outside of sdz, SnappedObject still != null, therefore sdz is visible all the way
            //if (db_sdzPatch.SnappedObject == null) ToggleSDZHighlighting(false);

            UpdateStates(DrillBitController.DrillBitState.IMPROPERLY_SNAPPED, DrillChuckState.PARTIALLY_OPENED);
        }
    }

    /// <summary>
    /// Indicates that the drill chuck was rotated until it is fully opened.
    /// <seealso cref="https://vrtoolkit.readme.io/docs/vrtk_rotatetransformgrabattach"/>
    /// </summary>
    /// <param name="sender"> This object </param>
    /// <param name="e"> Event payload <see cref="RotateTransformGrabAttachEventArgs"/> </param>
    private void MaxAngleReached(object sender, RotateTransformGrabAttachEventArgs e)
    {
        State = DrillChuckState.FULLY_OPENED;
        StartCoroutine(FlashDrillChuck(Color.green));
        gameObject.GetComponent<AudioSource>().PlayOneShot(openedClip);

        if (!db_sdz.gameObject.activeSelf && !bd_controller.drilling) db_sdz.gameObject.SetActive(true);
        ToggleSDZHighlighting(true);

        UpdateStates(DrillBitController.DrillBitState.IMPROPERLY_SNAPPED, DrillChuckState.FULLY_OPENED);
    }

    /// <summary>
    /// Indicates that the drill chuck was rotated until it is fully closed.
    /// <seealso cref="https://vrtoolkit.readme.io/docs/vrtk_rotatetransformgrabattach"/>
    /// </summary>
    /// <param name="sender"> This object </param>
    /// <param name="e"> Event payload <see cref="RotateTransformGrabAttachEventArgs"/> </param>
    private void MinAngleReached(object sender, RotateTransformGrabAttachEventArgs e)
    {
        State = DrillChuckState.FULLY_CLOSED;
        StartCoroutine(FlashDrillChuck(Color.red));
        gameObject.GetComponent<AudioSource>().PlayOneShot(closedClip);

        UpdateStates(DrillBitController.DrillBitState.PROPERLY_SNAPPED, DrillChuckState.FULLY_CLOSED);
    }

    /// <summary>
    /// Flashes the drill chuck in a specified color, as visual cue for the player
    /// <seealso cref="https://answers.unity.com/questions/13763/making-a-model-flash-a-certain-color-when-shot.html"/>
    /// </summary>
    /// <param name="flashColor"> The color in which the object flashes </param>
    private IEnumerator FlashDrillChuck(Color flashColor)
    {
        Material m = dc.GetComponent<MeshRenderer>().material;
        Color c = dc.GetComponent<MeshRenderer>().material.color;
        dc.GetComponent<MeshRenderer>().material = null;
        dc.GetComponent<MeshRenderer>().material.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        dc.GetComponent<MeshRenderer>().material = m;
        dc.GetComponent<MeshRenderer>().material.color = c;
    }

    /// <summary>
    /// Updates the states if the drill chuck was manipulated AFTER snapping the drill bit
    /// </summary>
    /// <param name="stateToCheck"> State to check to avoid triggering the update again after the state were adjusted </param>
    /// <param name="stateToUpdate"> State to send as parameter to decide how to update the drill bit state </param>
    private void UpdateStates(DrillBitController.DrillBitState stateToCheck, DrillChuckState stateToUpdate)
    {
        GameObject snappedObject = db_sdzPatch.SnappedObject;

        if (snappedObject != null && snappedObject.GetComponent<DrillBitController>().State != stateToCheck)
        {
            Debug.Log("...Updating...");
            snappedObject.GetComponent<DrillBitController>().UpdateSnappedBehaviour(stateToUpdate);
        }
    }

    /// <summary>
    /// En-/Disables the highlighhting object of the snap drop zone for the drill bit, according to the state of the drill chuck.
    /// </summary>
    /// <param name="openingDrillChuck"> Boolean to know if the drill chuck is currently getting opened or closed </param>
    public void ToggleSDZHighlighting(bool openingDrillChuck)
    {
        if (db_sdz.GetComponentInChildren<MeshRenderer>() != null && !bd_controller.drilling)
            db_sdz.GetComponentInChildren<MeshRenderer>().enabled = openingDrillChuck;
    }
}
