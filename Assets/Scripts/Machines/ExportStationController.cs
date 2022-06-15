using Challenges;
using System.Collections;
using UnityEngine;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class ExportStationController : MonoBehaviour
{
    private float origPressedDistance;
    private VRTK_ArtificialPusher confirmButton_artPush;
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private GameObject confirmButton_Parent;
    [SerializeField] private float confirmButtonCooldown = 3f;

    private void Awake()
    {
        confirmButton_artPush = (confirmButton_artPush == null ? confirmButton_Parent.GetComponent<VRTK_ArtificialPusher>() : confirmButton_artPush);
        confirmButton_artPush.MinLimitExited += MinLimitExited;

        origPressedDistance = confirmButton_artPush.pressedDistance;
    }

    private void MinLimitExited(object sender, ControllableEventArgs e)
    {
        if (sender.Equals(confirmButton_artPush))
        {
            GameObject turnedInObject = challengeManager.gameObject.GetComponentInChildren<SnapDropZonePatch>().SnappedObject;

            if (turnedInObject != null)
            {
                confirmButton_artPush.SetStayPressed(true);
                StartCoroutine(StartConfirmButtonCooldown());
                Debug.Log("Handed in to review");
                challengeManager.CurrentExerciseController.OnUserSubmit(challengeManager.EndIteration);
            }
        }
    }

    IEnumerator StartConfirmButtonCooldown()
    {
        yield return new WaitForSeconds(confirmButtonCooldown);
        confirmButton_artPush.SetStayPressed(false);
    }
}
