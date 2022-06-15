using System;
using Distractions.Management.EventSystem.DataContainer;
using Distractions.Management.EventSystem.EventDataContainer;
using Distractions.Management.EventSystem.Utility;
using Distractions.Models.Invokables;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

namespace Distractions.Management.EventSystem.Listener
{
    public class FlyListener : DistractionEventListener
    {
        [OdinSerialize]
        private FlyDistractionInvokable flyDistractionInvokable;

        public override void OnEventStart(DistractionEventData distractionEventData = default)
        {
            flyDistractionInvokable.InvokeDistraction(distractionEventData?.DistractionData);
        }

        public override void OnEventEnd() => flyDistractionInvokable.RevokeDistraction();

    }
}