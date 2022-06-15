using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VRTK;
using VRTK.Controllables.ArtificialBased;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField] GameObject[] gameObjectsToExclude;
    [SerializeField] VRTK_ArtificialPusher[] buttonPushers;

    private void Awake()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject gameObject in gameObjectsToExclude)
        {
            foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>())
            {
                if (!transform.gameObject.name.Contains("Index"))
                {
                    //Debug.Log("Excluded :" + transform.gameObject.name);
                    list.Add(transform.gameObject);
                }
            }
        }
        gameObjectsToExclude = list.ToArray();

        foreach (VRTK_ArtificialPusher artificialPusher in buttonPushers)
        {
            artificialPusher.ignoreCollisionsWith = gameObjectsToExclude;
        }
    }
}
