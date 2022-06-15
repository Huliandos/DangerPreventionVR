using System;
using System.Linq;
using UnityEngine;

namespace Drawing
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DrawableObject : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private Texture2D texture2D;
        private Vector2Int brushSize = new Vector2Int(5, 5);
        
        private void Awake()
        {
            SetupDrawableTexture();
            AdjustBrushSize();
        }

        private void SetupDrawableTexture()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Texture2D originalTexture;

            if (meshRenderer.material.shader == Shader.Find("Standard"))
            {
                originalTexture = meshRenderer.material.mainTexture as Texture2D;
                CloneTexture(originalTexture);
            }
            else
            {
                originalTexture = meshRenderer.material.GetTexture("_Albedo") as Texture2D;
                texture2D = originalTexture;
            }
        }

        //TODO: Unity sets convex attribute of mesh collider automatically to true for no specific reason
        //TODO: this only happens when the mesh was cut before
        private void LateUpdate()
        {
            if (meshCollider == null)
                meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null && meshCollider.convex)
                meshCollider.convex = false;
        }

        private void OnDestroy()
        {
            if (meshCollider != null)
                meshCollider.convex = true;
        }

        public void DrawAtPosition(Vector3 textureCoordinate, Color32 specifiedColor)
        {
            Color32[] colors = new Color32[brushSize.x * brushSize.y];
            
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = specifiedColor;
            }

            Vector2 pixelUV = new Vector2(textureCoordinate.x * texture2D.width,
                textureCoordinate.y * texture2D.height);

            pixelUV -= new Vector2(brushSize.x * 0.5f, brushSize.y * 0.5f);

            texture2D.SetPixels32((int) pixelUV.x, (int) pixelUV.y, brushSize.x, brushSize.y, colors);
            
            texture2D.Apply();
        }

        private void CloneTexture(Texture2D originalTexture)
        {
            if (originalTexture == null) return;

            texture2D = new Texture2D(originalTexture.width, originalTexture.height);
            texture2D.SetPixels(originalTexture.GetPixels());
            texture2D.Apply();
            
            meshRenderer.material.mainTexture = texture2D;
        }
        
        private void AdjustBrushSize()
        {
            Vector3 boundsSize = GetComponent<MeshFilter>().mesh.bounds.size;
            if (boundsSize.x > boundsSize.z)
                brushSize.y = (int) (brushSize.y * (1 / (boundsSize.z / boundsSize.x)));
            else
                brushSize.x = (int) (brushSize.x * (1 / (boundsSize.x / boundsSize.z)));
        }
    }
}