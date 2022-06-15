using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Distractions.Models.Invokables
{
    public class NPCJumpscareDistractionInvokable : DistractionInvokable
    {
        [SerializeField] private GameObject NPCPrefab;

        private GameObject NPC;

        public override void InvokeDistraction<T>(T distractionData)
        {
            base.InvokeDistraction(distractionData);

            Transform VRTK_SDKManager = FindObjectOfType<VRTK.VRTK_SDKManager>().transform;
            Transform VReyes = VRTK_SDKManager.GetComponentInChildren<Camera>().transform;

            Vector3 spawnPos = VReyes.transform.position - VReyes.transform.forward * .6f;
            spawnPos.y = 0;
            Quaternion spawnRot = Quaternion.LookRotation(new Vector3(VReyes.transform.position.x, 0, VReyes.transform.position.z) - spawnPos, Vector3.up);

            NPC = Instantiate(NPCPrefab, spawnPos, spawnRot);

            NPC.SetActive(true);
        }

        public override void RevokeDistraction()
        {
            base.RevokeDistraction();

            //Do nothing here. Destroying it feels odd gameflow wise
            //Destroy(NPC);
        }
    }
}
