using Distractions.Management.EventSystem.EventDataContainer;

namespace Distractions.Management.EventSystem.Listener
{
    public interface IEventListener
    {
        void OnEventTrigger(DistractionEventData distractionEventData);
        void OnEventStart(DistractionEventData distractionEventData);
        void OnEventEnd();
    }
}