using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillBitPSController : MonoBehaviour
{
    ParticleSystem ps;

    [SerializeField]
    bool metalSheetPs = true;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();   
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == Tags.splittable) { 

            if(metalSheetPs && !other.GetComponent<RootObject>())   //if this is a metal sheet particle system, then don't play when the player hand collides with me
                ps.Play(); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == Tags.splittable) { 
            ps.Stop(); 
        }
    }
}
