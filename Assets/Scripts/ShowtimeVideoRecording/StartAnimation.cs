using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Distractions.Management.EventSystem.Listener.Hotspots;

public class StartAnimation : MonoBehaviour
{
    [SerializeField]
    Animator waitForAnimator;

    bool checkAnimationDone;

    InvokableHotspot[] invokableHotspots;   //gets fetched in order: BenchDrill, Guillotine, HandLever

    [SerializeField]
    GameObject NPCprefab;

    // Start is called before the first frame update
    void Start()
    {
        if (waitForAnimator && waitForAnimator.GetComponent<Rigidbody>()) Destroy(waitForAnimator.GetComponent<Rigidbody>());

        invokableHotspots = FindObjectsOfType<InvokableHotspot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!checkAnimationDone)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (waitForAnimator != null)
                {
                    waitForAnimator.enabled = true;
                    checkAnimationDone = true;
                }
                else
                {
                    GetComponent<Animator>().enabled = true;
                }
            }
        }
        else {
            if (waitForAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !waitForAnimator.IsInTransition(0))
            {
                checkAnimationDone = false;
                GetComponent<Animator>().enabled = true;
                StartCoroutine(DistractionTriggerCoroutine());
            }
        }
    }

    IEnumerator DistractionTriggerCoroutine() {
        yield return new WaitForSeconds(10);    //10 seconds
        DestroyGuillotineSheer();

        yield return new WaitForSeconds(1.5f);   //11:30 secs
        TriggerHandLeaverLightFlicker();

        yield return new WaitForSeconds(3f);  //14.30secs
        Challenges.Timer.StartTimer.Invoke();

        yield return new WaitForSeconds(3.5f);  //18 secs
        TriggerRadioDistraction();
        SpawnFlyHead();
        SpawnFlyMachine();

        //yield return new WaitForSeconds(4f);  //22 secs

        yield return new WaitForSeconds(5.5f);  //23.5 secs
        SpawnNPC();
    }

    void DestroyGuillotineSheer() {
        invokableHotspots[1].DestroyGuillotineSheer();
    }

    void TriggerHandLeaverLightFlicker() {
        invokableHotspots[2].LightFlickerDistraction();
    }

    void TriggerRadioDistraction() {
        invokableHotspots[2].RadioRandomNoise();
    }

    void SpawnFlyHead() {
        invokableHotspots[0].FlyHeadDistraction();
    }

    void SpawnFlyMachine() {
        invokableHotspots[0].FlyMachineDistraction();
    }

    void SpawnNPC() {
        Vector3 spawnPos = transform.position - transform.forward*.7f;

        spawnPos.y = 0;

        GameObject NPC = Instantiate(NPCprefab, spawnPos, Quaternion.identity);

        NPC.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
