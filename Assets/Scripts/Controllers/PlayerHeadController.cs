using UnityEngine;

public class PlayerHeadController : MonoBehaviour
{
    #region private fields

    private PlayerController playerController;
    private GameController gameController;
    private EarBeeping earBeeping;
    private int hitCounter = 0;

    #endregion

    private void OnEnable()
    {
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (gameController == null) gameController = GameController.Instance;
        if (earBeeping == null) earBeeping = FindObjectOfType<EarBeeping>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitCounter < 1)
        {
            if (other.gameObject.CompareTag("HandLevelShearsLever"))
            {
                if (!playerController.HelmetIsOn)
                {
                    hitCounter++;

                    // head smash
                    playerController.PlayHeadSmashSound();

                    // vfx
                    playerController.DisplayInjuryVFX(false);

                    // heartbeat
                    playerController.PlayHeartbeatSound(true);

                    // earbeeping
                    earBeeping.StartBeepingLoop(false);

                    // game over with reduced timeUntilReload value
                    gameController.GameOver(Injury.Bonking, true);
                }
                else
                {
                    playerController.PlayHeadSmashSound();
                }
            }
        }
    }
}