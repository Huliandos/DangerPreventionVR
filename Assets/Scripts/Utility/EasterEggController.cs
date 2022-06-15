using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class EasterEggController : MonoBehaviour
{
    [SerializeField] VRTK_HeadsetFade headset;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "CustomLiquid04")
        {
            GetComponent<AudioSource>().Play();
            Invoke("Reload", 1.5f);
        }
    }

    private void Reload()
    {
        headset.Fade(Color.black, 3f);
        SceneManager.LoadScene(0);
    }
}
