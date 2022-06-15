using Distractions.Management.EventSystem.DataContainer;
using Sirenix.OdinInspector;

namespace Distractions.Models.Invokables
{
    public class DistractionInvokable : SerializedMonoBehaviour, IDistractionInvokable
    {
        public virtual void InvokeDistraction<T>(T distractionData) where T : DistractionData
        {
        }

        public virtual void RevokeDistraction()
        {
        }
    }
}