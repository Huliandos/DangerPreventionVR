using Distractions.Management.EventSystem.DataContainer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Distractions.Models.Invokables
{
    public class FlyDistractionInvokable : DistractionInvokable
    {
        [SerializeField] private Transform playerHead, machineSpawnpoint;

        [SerializeField] private GameObject flyPrefab, flyHeadPrefab;

        private GameObject flyObject;

        public void ChooseRandomFlySpawnLocation(int randomSpawnpoint = -1)
        {
            if (randomSpawnpoint == -1)
                randomSpawnpoint = Random.Range(0, 2);

            if (randomSpawnpoint == 0)
            {
                flyObject = Instantiate(flyHeadPrefab, playerHead.position, playerHead.rotation);
                flyObject.transform.parent = playerHead.transform;
            }
            else
            {
                flyObject = Instantiate(flyPrefab, machineSpawnpoint.position, machineSpawnpoint.rotation);
                flyObject.transform.parent = machineSpawnpoint.transform;
            }

            flyObject.SetActive(true);
        }

        public override void InvokeDistraction<T>(T distractionData)
        {
            base.InvokeDistraction(distractionData);

            ChooseRandomFlySpawnLocation();
        }

        public override void RevokeDistraction()
        {
            base.RevokeDistraction();

            //Do nothing here. Destroying it feels odd gameflow wise
            //Destroy(flyObject);
        }
    }
}