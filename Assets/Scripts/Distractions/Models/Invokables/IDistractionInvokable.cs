using Distractions.Management.EventSystem.DataContainer;

namespace Distractions.Models.Invokables
{
    public interface IDistractionInvokable
    {
        void InvokeDistraction<T>(T distractionData) where T : DistractionData;
        void RevokeDistraction();
    }
}