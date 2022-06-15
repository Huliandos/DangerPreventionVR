using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnTexture : MonoBehaviour
{
    [SerializeField]
    int brushSize = 12;

    [SerializeField]
    Color drawColor = new Color(.87f, 1, .76f);

    Texture2D texture;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        
        //Use this to find out the Shaders Texture Map names
        /*
        string[] names = rend.material.GetTexturePropertyNames();
        for (int i=0; i<names.Length; i++) {
            Debug.Log("Material Num: " + i + " Name: " + names[i]);
        }
        */

        // duplicate the original texture and assign to the material
        //texture = Instantiate(rend.material.mainTexture) as Texture2D;
        texture = Instantiate(rend.material.GetTexture("_Albedo")) as Texture2D;
        rend.material.SetTexture("_Albedo", texture);
    }

    public void Draw(Vector2 textureCoord) {
        Debug.Log("Texture Coords: " + textureCoord);

        Vector2 pixelUV = textureCoord;
        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;

        Debug.Log("PixelUV Coords: " + pixelUV);

        Color[] colors = new Color[brushSize * brushSize];

        // set brush to glue gun glue color (greenish white)
        for (var i = 0; i < brushSize * brushSize; i++)
        {
            colors[i] = drawColor;
        }


        texture.SetPixels((int)pixelUV.x, (int)pixelUV.y, brushSize, brushSize, colors);

        texture.Apply();
    }
}
