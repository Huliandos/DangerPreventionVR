using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GlueGunController : MonoBehaviour
{
    [SerializeField]
    ParticleSystem glueParticles;

    [SerializeField]
    Transform glueGunTip;

    VRTK_ControllerEvents controllerEvents;
    Animator animator;

    bool shootGlue, isGrabbed;

    private void Start()
    {
        GetComponent<VRTK_InteractableObject>().InteractableObjectGrabbed += InteractableObjectGrabbed;
        GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Gluegun-Trigger Control
        if (isGrabbed)
        {
            float axisValue = controllerEvents.GetTriggerAxis();

            animator.SetFloat("Trigger", axisValue);
            if (axisValue >= .9f)
            {
                shootGlue = true;
            }
            else if (axisValue < .9f)
            {
                shootGlue = false;
            }
        }
        else
        {
            animator.SetFloat("Trigger", 0);
            shootGlue = false;
        }


        //Glue shooting logic
        if (shootGlue)
        {
            if (glueParticles != null && !glueParticles.isPlaying) glueParticles.Play();

            RaycastHit hit;
            Physics.Raycast(glueGunTip.position, transform.forward, out hit, .1f);
            //Debug.DrawLine(glueGunTip.position, glueGunTip.position+transform.forward*.1f);


            //if Raycast hit an Object AND hit collider is an object tagged splittable AND the hit object isn't one of the hands bones AND other object isn't sticky yet
            if (hit.collider && hit.collider.gameObject.tag == Tags.splittable && !hit.collider.gameObject.GetComponent<RootObject>()) {
                //Apply Sticky Object Script
                if (!hit.collider.gameObject.GetComponent<StickyObject>())
                {
                    hit.collider.gameObject.AddComponent<StickyObject>();
                }

                //Draw on Texture
                if (hit.collider.gameObject.GetComponent<DrawOnTexture>())
                {
                    hit.collider.gameObject.GetComponent<DrawOnTexture>().Draw(hit.textureCoord);
                }

            }
        }
        else {
            if (glueParticles != null && glueParticles.isPlaying) glueParticles.Stop();
        }
    }

    void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        isGrabbed = true;
        controllerEvents = e.interactingObject.GetComponent<VRTK_ControllerEvents>(); ;
    }

    void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        isGrabbed = false;
        controllerEvents = null;
    }
}
