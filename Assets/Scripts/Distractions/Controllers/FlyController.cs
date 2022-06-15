using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ToDo: Delete. Becomes obsolete once new system is reviewed
public class FlyController : MonoBehaviour
{
    [SerializeField]
    bool willLand = false;

    //Animation[] animations = new Animation[2];
    Animator flyAnimator, flyRotationAnimator, flyUpDownAnimator;

    Rigidbody rb;

    [SerializeField]
    int numOfCirclesBeforeSittdownAnimation = 5;

    float circleAnimationLength, circleLandAnimationLength;

    // Start is called before the first frame update
    void Start()
    {
        Transform child = transform.GetComponentInDirectChildren<Transform>();

        //animations[0] = GetComponent<Animation>();
        //animations[1] = child.GetComponent<Animation>();

        //circleAnimationLength = animations[0].clip.length;

        flyRotationAnimator = GetComponent<Animator>();
        flyUpDownAnimator = child.GetComponent<Animator>();
        flyAnimator = child.GetComponentInDirectChildren<Transform>().GetComponent<Animator>();

        AnimationClip[] clips = flyRotationAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "FlyRotation") {
                circleAnimationLength = clip.length;
            }
            else if (clip.name == "FlyRotationLanding" || clip.name == "FlyRotationCloser")
            {
                circleLandAnimationLength = clip.length;
            }
        }

        flyRotationAnimator.Play("FlyRotation");
        flyUpDownAnimator.Play("FlyUpDown");
        flyAnimator.Play("Flying");

        rb = GetComponent<Rigidbody>();

        StartCoroutine(Land());
    }

    IEnumerator Land()
    {
        yield return new WaitForSeconds(circleAnimationLength * numOfCirclesBeforeSittdownAnimation);

        if(willLand) transform.GetComponentInDirectChildren<AudioSource>().Stop();

        flyRotationAnimator.SetBool("Land", true);
        flyUpDownAnimator.SetBool("Land", true);
        flyAnimator.SetBool("Land", true);

        yield return new WaitForSeconds(circleLandAnimationLength);

        if (flyAnimator.GetBool("isDying") == false)
        {
            if(willLand) transform.GetComponentInDirectChildren<AudioSource>().Play();

            flyRotationAnimator.SetBool("Land", false);
            flyUpDownAnimator.SetBool("Land", false);
            flyAnimator.SetBool("Land", false);
        }

        StartCoroutine(Land());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponentInChildren<RootObject>() != null) { //A way to detect the player hand rn. This behaves weirdly otherwise.
            //foreach (Animation anim in animations){
            //    Destroy(anim);
            //}

            //flyRotationAnimator.StopPlayback();
            //flyUpDownAnimator.StopPlayback();
            Destroy(flyRotationAnimator);
            Destroy(flyUpDownAnimator);

            transform.GetComponentInDirectChildren<AudioSource>().Stop();

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;

            flyAnimator.SetBool("IsDying", true);

            StartCoroutine(WaitThenDestroy());
        }
    }

    IEnumerator WaitThenDestroy() {
        yield return new WaitForSeconds(5);

        Destroy(gameObject);
    }
}
