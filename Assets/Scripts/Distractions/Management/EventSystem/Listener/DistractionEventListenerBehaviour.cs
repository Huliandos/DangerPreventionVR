using System.Collections.Generic;
using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Management.EventSystem.Utility;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Distractions.Management.EventSystem.Listener
{
    /// <summary>
    /// Abstract Super Class for all distraction listener objects in the scene
    /// </summary>
    public abstract class DistractionEventListenerBehaviour : SerializedMonoBehaviour, IEventListener
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
        
        public virtual void OnEventTrigger(DistractionEventData distractionEventData)
        {
        }

        public virtual void OnEventStart(DistractionEventData distractionEventData)
        {
        }

        public virtual void OnEventEnd()
        {
        }
    }
}