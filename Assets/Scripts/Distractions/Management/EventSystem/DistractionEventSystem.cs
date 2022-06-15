using System;
using System.Collections.Generic;
using System.Linq;
using Distractions.Management.EventSystem.DataContainer;
using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Management.EventSystem.Listener;
using Distractions.Management.EventSystem.Utility;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Distractions.Management.EventSystem
{
    /// <summary>
    /// Distraction Event System
    /// registers Event Listeners that are able to listen to incoming Distraction events
    /// dispatches raised Distraction Events called from anywhere
    /// </summary>
    public class DistractionEventSystem : SerializedMonoBehaviour
    {
        [OdinSerialize, ShowInInspector]
        public List<DistractionEventListenerBehaviour> DistractionEventListeners
        {
            get => distractionEventListeners;
            set => distractionEventListeners = value;
        }

        [HideInInspector] public static List<DistractionEventListenerBehaviour> distractionEventListeners = new List<DistractionEventListenerBehaviour>();
        private static List<Reason> OnGoingEvents = new List<Reason>();
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        /// <summary>
        /// registers a new Distraction Event Listener if not already present
        /// please register on Start() if done on runtime
        /// </summary>
        /// <param name="listener">the listener instance to register</param>
        /// <param name="reason">which reason the listener is listening on</param>
        public static void RegisterDistractionEventListener(DistractionEventListenerBehaviour listener, Reason reason)
        {
            if (!distractionEventListeners.Contains(listener))
                distractionEventListeners.Add(listener);
        }

        public static void UnregisterDistractionEventListener(DistractionEventListenerBehaviour listener, Reason reason)
        {
            if (distractionEventListeners.Contains(listener))
                distractionEventListeners.Remove(listener);
        }

        /// <summary>
        /// Method for Raising distraction event from anywhere
        /// </summary>
        /// <param name="reason">what was done to invoke a distraction event</param>
        /// <param name="eventType">Type of distraction event</param>
        /// <param name="distractionData">data that has to be forwarded to the listeners (needed for teacher mode)</param>
        public static void RaiseDistractionEvent(Reason reason, DistractionEventType eventType, DistractionData distractionData = default)
        {
            switch (eventType)
            {
                case DistractionEventType.Start when OnGoingEvents.Contains(reason):
                    return;
                case DistractionEventType.Start:
                    OnGoingEvents.Add(reason);
                    break;
                case DistractionEventType.End when !OnGoingEvents.Contains(reason):
                    return;
                case DistractionEventType.End:
                    OnGoingEvents.Remove(reason);
                    break;
            }

            DistractionEventData distractionEventData = new DistractionEventData(reason);

            DispatchDistractionEvent(eventType, distractionEventData);
        }

        /// <summary>
        /// Overload Method for Raising distraction event from teacher
        /// </summary>
        /// <param name="distractionType">Type of the distraction that should be triggered</param>
        /// <param name="distractionData">data that has to be forwarded to the listeners (needed for teacher mode)</param>
        public static void RaiseDistractionEvent(DistractionType distractionType, DistractionData distractionData)
        {
            DistractionEventData distractionEventData = new DistractionEventData(distractionData);
            
            DispatchDistractionEvent(distractionType, distractionEventData);
        }

        /// <summary>
        /// dispatches raised events to the appropriate listener
        /// </summary>
        /// <param name="eventType">type of event that has been raised</param>
        /// <param name="data">event data that has to be forwarded</param>
        private static void DispatchDistractionEvent(DistractionEventType eventType, DistractionEventData data)
        {
            foreach (DistractionEventListenerBehaviour listener in distractionEventListeners)
            {
                if (!listener.Reasons.Contains(data.Reason))
                    continue;
                
                switch (eventType)
                {
                    case DistractionEventType.Trigger:
                        listener.OnEventTrigger(data);
                        break;
                    case DistractionEventType.Start:
                        listener.OnEventStart(data);
                        break;
                    case DistractionEventType.End:
                        listener.OnEventEnd();
                        break;
                }
            }
        }

        /// <summary>
        /// Overload Method that dispatches raised events to the appropriate listener
        /// </summary>
        /// <param name="distractionType">Type of the distraction that has to be triggered</param>
        /// <param name="data">forwarded data to the distraction</param>
        private static void DispatchDistractionEvent(DistractionType distractionType, DistractionEventData data)
        {
            distractionEventListeners.FirstOrDefault(listener => listener.DistractionType == distractionType)?.OnEventTrigger(data);
        }
    }
}