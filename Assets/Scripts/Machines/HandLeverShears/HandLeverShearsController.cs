using Distractions.Management.EventSystem;
using Distractions.Management.EventSystem.DataContainer;
using Distractions.Management.EventSystem.Utility;
using UnityEngine;
using VRTK.Controllables.ArtificialBased;

public class HandLeverShearsController : MonoBehaviour
{
    #region private fields

    Animator handleAnimator;
    bool hasCut;
    private bool ongoingDistraction = false;
    private float distractionFireCooldown = 1f;
    private float distractionTimer;

    #endregion

    #region serialized fields

    [SerializeField] PlayerHeadController playerHeadController;
    [SerializeField] SplittingPlaneHand splittingPlane;
    [SerializeField] VRTK_ArtificialRotator handleContainer, hookContainer;

    #endregion

    #region public fields

    public AudioClip handCutClip;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        handleAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //if hook is opened
        if (hookContainer.GetNormalizedValue() < .8f)
        {
            //DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.Start);
            // enable head trigger collider
            playerHeadController.gameObject.GetComponent<SphereCollider>().enabled = true;

            if (handleContainer.isLocked) handleContainer.isLocked = false;

            float handleNormPos = handleContainer.GetNormalizedValue();
            handleAnimator.SetFloat("NormCutValue", handleNormPos);

            if (!ongoingDistraction && Time.time >= distractionTimer + distractionFireCooldown && handleNormPos > 0.1f)
            {
                Debug.Log("Start if entered");

                distractionTimer = Time.time;

                DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.Start);
                ongoingDistraction = true;
            }

            if (ongoingDistraction && handleNormPos < 0.1f) 
            {
                Debug.Log("Distraction Event System stopped");

                distractionTimer = Time.time;

                DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.End);
                ongoingDistraction = false;
            }

            if (!hasCut && handleNormPos >= .95f)
            {
                hasCut = true;
                splittingPlane.Cut();
                GetComponent<AudioSource>().Play();
            }
            else if (hasCut && handleNormPos < .3f)
            {
                hasCut = false;
            }
        }
        //if hook is closed and handle is at it's hookable position
        else if (handleContainer.GetNormalizedValue() <= 0.05f)
        {
            //DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.End);
            handleContainer.isLocked = true;

            // disable head trigger collider
            playerHeadController.gameObject.GetComponent<SphereCollider>().enabled = false;
        }
        //if hook is close and handle is not in its hookable position
        else
        {
            if (handleContainer.isLocked) handleContainer.isLocked = false;

            // enable head trigger collider
            playerHeadController.gameObject.GetComponent<SphereCollider>().enabled = true;

            float handleNormPos = handleContainer.GetNormalizedValue();
            handleAnimator.SetFloat("NormCutValue", handleNormPos);

            if (!ongoingDistraction && Time.time >= distractionTimer + distractionFireCooldown && handleNormPos > 0.1f)
            {
                Debug.Log("Start if entered");

                distractionTimer = Time.time;

                DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.Start);
                ongoingDistraction = true;
            }

            if (ongoingDistraction && handleNormPos < 0.1f)
            {
                Debug.Log("Distraction Event System stopped");

                distractionTimer = Time.time;

                DistractionEventSystem.RaiseDistractionEvent(Reason.UsageHandLeverShears, DistractionEventType.End);
                ongoingDistraction = false;
            }

            if (!hasCut && handleNormPos >= .95f)
            {
                hasCut = true;
                splittingPlane.Cut();
            }
            else if (hasCut && handleNormPos < .3f)
            {
                hasCut = false;
            }
        }
    }
}
