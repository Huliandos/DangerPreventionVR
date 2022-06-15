using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CutOnTriggerEnter : MonoBehaviour
{
    [SerializeField]
    SplittingPlaneHand splittingPlaneHand;

    [SerializeField]
    Animation animation;

    float betweenTouchCd;

    public bool testBool;

    private void Update()
    {
        if (testBool) {
            Debug.Log("Test Bool triggered");
            testBool = false;

            if(animation != null) animation.Play();

            splittingPlaneHand.MultiThreadCut();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (betweenTouchCd<Time.time && other.tag == Tags.splittable) {
            betweenTouchCd = Time.time + 2; //2 sec cooldown

            Debug.Log("Entered Button collider");

            if (animation != null) animation.Play();

            splittingPlaneHand.MultiThreadCut();

        }
    }
}
