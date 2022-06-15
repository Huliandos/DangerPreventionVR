using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ToDo: Delete. Becomes obsolete once new system is reviewed
public class FlyControllerPlayerHead : MonoBehaviour
{
    Animation[] animations = new Animation[2];
    Animator flyAnimator;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        Transform child = transform.GetComponentInDirectChildren<Transform>();

        animations[0] = GetComponent<Animation>();
        animations[1] = child.GetComponent<Animation>();

        flyAnimator = child.GetComponentInDirectChildren<Transform>().GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponentInChildren<RootObject>() != null) { //A way to detect the player hand rn. This behaves weirdly otherwise.
            foreach (Animation anim in animations){
                Destroy(anim);
            }

            transform.parent = null;    //to deparent from player head, so that movement isn't affected by it anymore

            transform.GetComponentInDirectChildren<AudioSource>().Stop();

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;

            flyAnimator.SetBool("IsDying", true);

            StartCoroutine(waitThenDestroy());
        }
    }

    IEnumerator waitThenDestroy() {
        yield return new WaitForSeconds(5);

        Destroy(gameObject);
    }
}
