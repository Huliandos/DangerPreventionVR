using UnityEngine;

public class SwapMaterialOnTriggerEnter: MonoBehaviour
{
    [SerializeField] Shader damageFXShader_workpiece;
    [SerializeField] Shader damageFXShader_playerHands;

    [SerializeField] Material workpieceDrillMat;
    [SerializeField] Material handDrillMat;

    private void OnTriggerEnter(Collider other)
    {
        /*
        if (other.gameObject.name == "Workpiece_metal") //ToDo: Change for new workpieces to work
        {
            Shader workpieceShader = other.gameObject.GetComponent<Renderer>().material.shader;
            if (workpieceShader != damageFXShader_workpiece)
            {
                workpieceShader = damageFXShader_workpiece;
                Debug.Log("Swapped shader " + workpieceShader + " to " + damageFXShader_workpiece);
            }

        }
        else if (other.gameObject.layer == 11) //TODO: hacky, layer 11 == "PlayerHands"
        {
            Shader handShader = other.gameObject.GetComponent<RootObject>().getRoot().transform.GetComponentInDirectChildren<Renderer>().material.shader;
            if (handShader != damageFXShader_playerHands)
            {
                handShader = damageFXShader_playerHands;
                Debug.Log("Swapped shader " + handShader + " to " + damageFXShader_playerHands);
            }
        }
        */
        if (other.gameObject.tag == Tags.splittable) {
            Material material;
            if (other.GetComponent<RootObject>())   //if mesh is player hand
            {
                material = other.gameObject.GetComponent<RootObject>().getRoot().transform.GetComponentInDirectChildren<Renderer>().material;

                if (!CompareMaterialNames(material, handDrillMat))
                {
                    Debug.Log("Swapped shader " + other.gameObject.GetComponent<RootObject>().getRoot().transform.GetComponentInDirectChildren<Renderer>().material + " to " + handDrillMat);
                    other.gameObject.GetComponent<RootObject>().getRoot().transform.GetComponentInDirectChildren<Renderer>().material = handDrillMat;
                }
            }
            else if(other.GetComponent<Utility.MoveGOPivot>())
            {
                material = other.gameObject.GetComponent<Renderer>().material;

                if (!CompareMaterialNames(material, workpieceDrillMat) && !CompareMaterialNames(material, handDrillMat))
                {

                    Debug.Log("Swapped shader " + other.gameObject.GetComponent<Renderer>().material + " to " + workpieceDrillMat);
                    Renderer rendererOther = other.gameObject.GetComponent<Renderer>();
                    Texture2D originalTexture = rendererOther.material.GetTexture("_MainTex") as Texture2D;
                    
                    rendererOther.material = workpieceDrillMat;
                    CloneAlbedoTexture(rendererOther, originalTexture);
                    
                    other.gameObject.GetComponent<DamageFX>().MaterialSwapped();
                }
            }
        }
    }

    private static void CloneAlbedoTexture(Renderer renderer, Texture2D originalTexture)
    {
        if (originalTexture == null) return;
        
        Texture2D texture2D = new Texture2D(originalTexture.width, originalTexture.height);
        texture2D.SetPixels(originalTexture.GetPixels());
        texture2D.Apply();

        renderer.material.SetTexture("_Albedo", texture2D);
    }

    //checking if names are similar and then again by removing excess name on both sides: E.g. (Instance) tag
    bool CompareMaterialNames(Material materialA, Material materialB)
    {
        if (materialA.name == materialB.name)
            return true;

        if (materialA.name.Length > materialB.name.Length && materialA.name.Substring(0, materialB.name.Length) == materialB.name)
            return true;

        if (materialA.name.Length < materialB.name.Length && materialB.name.Substring(0, materialA.name.Length) == materialA.name)
            return true;

        return false;
    }
}
