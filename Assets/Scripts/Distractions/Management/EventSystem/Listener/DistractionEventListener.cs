using System;
using System.Collections.Generic;
using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Management.EventSystem.Utility;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Distractions.Management.EventSystem.Listener
{
    /// <summary>
    /// Abstract Super Class for all Distraction Cases that can occur
    /// Subclass is the glue that is packing data for the Distraction Invokable
    /// </summary>
    public abstract class DistractionEventListener : IEventListener
    {
        public List<Reason> Reasons
        {
            get => reasons;
            set => reasons = value;
        }

        public DistractionType DistractionType
        {
            get => distractionType;
            set => distractionType = value;
        }

        [OdinSerialize, ShowInInspector]
        private List<Reason> reasons = new List<Reason>();

        [OdinSerialize, ShowInInspector]
        private DistractionType distractionType;
        
        public virtual void OnEventTrigger(DistractionEventData distractionEventData = default)
        {
        }

        public virtual void OnEventStart(DistractionEventData distractionEventData = default)
        {
        }

        public virtual void OnEventEnd()
        {
        }
    }
}