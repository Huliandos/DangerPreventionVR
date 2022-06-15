using System.Collections;
using UnityEngine;

public class SpawnBlood : MonoBehaviour
{
    #region private fields

    private PlayerController playerController;
    private GameObject[] bloodPrefabs;

    #endregion

    #region public fields

    public SetBloodPrefabSettings setBloodPrefabSettings;

    #endregion

    private void OnEnable()
    {
        playerController = FindObjectOfType<PlayerController>();

        // triggered when hand was cut. Define settings for bleeding from wound
        if (setBloodPrefabSettings == null)
        {
            setBloodPrefabSettings = FindObjectOfType<SetBloodPrefabSettings>();
            //setBloodPrefabSettings.GroundHeight = 0f;
            //setBloodPrefabSettings.scaling = new Vector3(0.1f, 0.1f, 0.1f);
            //setBloodPrefabSettings.AnimationSpeed = 4f;
        }

        PreparePrefabs();
    }

    private void PreparePrefabs()
    {
        bloodPrefabs = Resources.LoadAll<GameObject>("BloodPrefabs");

        if (bloodPrefabs.Length != 0)
        {
            foreach (GameObject bloodPrefab in bloodPrefabs)
            {
                BFX_BloodSettings bloodSettings = bloodPrefab.GetComponent<BFX_BloodSettings>();

                bloodSettings.AnimationSpeed = setBloodPrefabSettings.AnimationSpeed;
                bloodSettings.GroundHeight = setBloodPrefabSettings.GroundHeight;
                bloodSettings.LightIntensityMultiplier = setBloodPrefabSettings.LightIntensityMultiplier;
                bloodSettings.FreezeDecalDisappearance = setBloodPrefabSettings.FreezeDecalDisappearance;
                bloodSettings.DecalRenderinMode = (BFX_BloodSettings._DecalRenderinMode)setBloodPrefabSettings.DecalRenderinMode;
                bloodSettings.ClampDecalSideSurface = setBloodPrefabSettings.ClampDecalSideSurface;

                bloodPrefab.transform.localScale = setBloodPrefabSettings.scaling;
            }
        }
    }

    // Credits: method adapted from "BFX_DemoTest" script of bought asset VolumetricBloodFX (by KriptoFX aka kripto289)
    public void Bleed(Vector3 bleedPoint, Vector3 bleedDirection)
    {
        float angle = Mathf.Atan2(bleedDirection.x, bleedDirection.z) * Mathf.Rad2Deg + 180;

        var effectIdx = Random.Range(0, bloodPrefabs.Length);
        if (effectIdx == bloodPrefabs.Length) effectIdx = 0;

        Instantiate(bloodPrefabs[effectIdx], bleedPoint, Quaternion.Euler(0, angle + 90, 0));
    }

    public IEnumerator InvokeBleed()
    {
        while (true)
        {
            Transform childTransform = transform.GetComponentInDirectChildren<Transform>();

            Vector3 bleedDirection = childTransform.TransformDirection(-childTransform.forward);
            Bleed(childTransform.position, bleedDirection);
            playerController.PlayHeartbeatSound(false);

            yield return new WaitForSeconds(0.8f);
        }
    }

    public void InvokeBleedFromCutterClass(Vector3 invokeBleedDirection)
    {
        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.transform.position = GetComponent<Renderer>().bounds.ClosestPoint(GetComponent<Renderer>().bounds.center + (invokeBleedDirection * 2));

        Quaternion.RotateTowards(go.transform.rotation, Quaternion.Euler(invokeBleedDirection), 0.01f);

        StartCoroutine(InvokeBleed());
    }
}
