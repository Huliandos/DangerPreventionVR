using System.Collections;
using UnityEngine;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class ButtonPressed : MonoBehaviour
{
    #region private fields

    private int startknopfPressedCounter;

    #endregion

    #region serialized fields

    [SerializeField] GuillotineShearsController guillotineShearsController;
    [SerializeField] VRTK_BaseControllable controllable;
    [SerializeField] Animation animation;
    [SerializeField] SplittingPlaneHand splittingPlaneHand;
    [SerializeField] EarBeeping earBeeping;
    [SerializeField] PlayerController playerController;
    [SerializeField] int startknopfPressedThreshold;

    #endregion

    protected virtual void OnEnable()
    {
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        controllable.MaxLimitReached += MaxLimitReached;
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if (guillotineShearsController.startButtonUnlocked)
        {
            gameObject.GetComponentInParent<VRTK_ArtificialPusher>().SetStayPressed(true);
            StartCoroutine(StartStartButtonCooldown());

            animation.Play();

            GetComponent<AudioSource>().Play();

            splittingPlaneHand.MultiThreadCut();

            if (!playerController.EarmuffsAreOn)
            {
                startknopfPressedCounter++;
            }
        }

        if (startknopfPressedCounter == startknopfPressedThreshold)
        {
            earBeeping.StartBeepingLoop(true);
        }
    }

    IEnumerator StartStartButtonCooldown()
    {
        yield return new WaitForSeconds(guillotineShearsController.startButtonCooldown);
        gameObject.GetComponentInParent< VRTK_ArtificialPusher>().SetStayPressed(false);
    }
}
