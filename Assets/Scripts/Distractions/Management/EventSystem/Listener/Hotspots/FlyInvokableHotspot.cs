using System;
using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Models.Invokables;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


//ToDo: Delete. Becomes obsolete once new system is reviewed
namespace Distractions.Management.EventSystem.Listener.Hotspots
{
    public class FlyInvokableHotspot : DistractionInvokableHotspot
    {
        [SerializeField]
        private FlyDistractionInvokable flyDistractionInvokable;

        public override void OnEventStart(DistractionEventData distractionEventData)
        {
            flyDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
        }

        public override void OnEventEnd() => flyDistractionInvokable.RevokeDistraction();
    }
}