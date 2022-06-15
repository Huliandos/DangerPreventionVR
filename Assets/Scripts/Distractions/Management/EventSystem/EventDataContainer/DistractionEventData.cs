using Distractions.Management.EventSystem.DataContainer;
using Distractions.Management.EventSystem.Utility;

namespace Distractions.Management.EventSystem.EventDataContainer
{
    /// <summary>
    /// Data Object for transporting Distraction Data and the appropriate Reason for event call in it
    /// </summary>
    public class DistractionEventData
    {
        public Reason Reason => reason;
        public DistractionData DistractionData => distractionData;

        private Reason reason;
        private DistractionData distractionData;

        public DistractionEventData(Reason reason, DistractionData eventData = default)
        {
            this.reason = reason;
            this.distractionData = eventData;
        }

        public DistractionEventData(DistractionData eventData)
        {
            this.distractionData = eventData;
        }

        public DistractionEventData()
        {
        }
    }
}