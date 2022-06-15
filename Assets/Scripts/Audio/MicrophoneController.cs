using System;
using System.Linq;
using UnityEngine;

namespace Audio
{
    public class MicrophoneController : MonoBehaviour
    {
        [SerializeField] private string microphoneName = "Built-in Microphone";
        [SerializeField] private bool shouldRecord = true;
        
        private AudioSource microphoneAudioSource;
        
        private void Awake()
        {
            microphoneAudioSource = GetComponent<AudioSource>();
            
            if(Microphone.devices.Length <= 0)  
            {  
                Debug.LogWarning("No Microphone connected!");  
                return;
            }

            if (!Microphone.devices.Contains(microphoneName)) microphoneName = null;
            
            StartRecording();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                shouldRecord = !shouldRecord;
            
            if(shouldRecord && Microphone.IsRecording(microphoneName) 
               || !shouldRecord && !Microphone.IsRecording(microphoneName)) return;
            
            if(!shouldRecord && Microphone.IsRecording(microphoneName)) Microphone.End(microphoneName);
            if (shouldRecord && !Microphone.IsRecording(microphoneName)) StartRecording();
        }

        private void StartRecording()
        {
            Debug.Log("start recording");
            //microphoneAudioSource.clip = Microphone.Start(microphoneName, true, 10, 52000);
            //microphoneAudioSource.clip = Microphone.Start(microphoneName, true, 10, 48000);
           // microphoneAudioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            microphoneAudioSource.Play();
        }
    }
}