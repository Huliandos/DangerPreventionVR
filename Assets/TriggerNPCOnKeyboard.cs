using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Distractions.Management.EventSystem.Listener.Hotspots;

public class TriggerNPCOnKeyboard : MonoBehaviour
{
    [SerializeField]
    InvokableHotspot ih;

    bool keyPressed;

    // Start is called before the first frame update
    void Start()
    {
        ih = FindObjectOfType<InvokableHotspot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !keyPressed) {
            StartCoroutine(StartSpawnAfterSecs(3));

            keyPressed = true;
        }
    }

    IEnumerator StartSpawnAfterSecs(int seconds) {
        yield return new WaitForSeconds(seconds);

        ih.TriggerNPC();
    }
}
