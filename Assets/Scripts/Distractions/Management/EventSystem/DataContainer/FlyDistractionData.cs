using UnityEngine;

namespace Distractions.Management.EventSystem.DataContainer
{
    /// <summary>
    /// Class that stores data from events that results from
    /// Opening the hook of the Hand Lever Shears
    /// </summary>
    public class FlyDistractionData : DistractionData
    {
        public Vector3 SpawnPosition => spawnPosition;
        public Vector3 TargetPosition => targetPosition;

        private Vector3 spawnPosition;
        private Vector3 targetPosition;

        public FlyDistractionData(Vector3 spawnPosition = default, Vector3 targetPosition = default)
        {
            this.spawnPosition = spawnPosition;
            this.targetPosition = targetPosition;
        }
    }
}