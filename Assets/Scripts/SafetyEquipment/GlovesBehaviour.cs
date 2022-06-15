using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GlovesBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    private Mesh newMesh;
    void Start()
    {
        var use = GetComponent<VRTK_InteractableObject>();
        use.InteractableObjectUsed += Use_InteractableObjectUsed;

        newMesh = GetComponent<MeshFilter>().mesh;
    }

    private void Use_InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        if(e.interactingObject.tag == "PlayerControler")
        {
            e.interactingObject.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMesh = newMesh;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
