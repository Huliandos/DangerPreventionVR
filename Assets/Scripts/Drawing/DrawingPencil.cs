using System;
using UnityEngine;
using Utility;

namespace Drawing
{
    public class DrawingPencil : MonoBehaviour
    {
        [SerializeField] private Transform pencilFront, pencilBack;
        [SerializeField] private float drawingLength = 1f;
        [SerializeField] private Color32 drawingColor = Color.black;

        public bool enableDrawing = true;

        private Vector3 penDrawingDirection;

        private void Awake()
        {
            if (pencilBack == null || pencilFront == null)
            {
                Debug.Log("Pencil is missing referenced objects.");
                enableDrawing = false;
            }
        }

        private void LateUpdate()
        {
            if (!enableDrawing) return;
            
            penDrawingDirection = pencilFront.position - pencilBack.position;

            RaycastHit raycastHit;

            //Debug.DrawRay(pencilFront.position, penDrawingDirection * drawingLength, Color.green, 10000);
            if (!Physics.Raycast(pencilBack.position, penDrawingDirection, out raycastHit, (drawingLength + penDrawingDirection.magnitude), LayerMask.GetMask("InteractableObjects"))) 
                return;

            DrawableObject drawableObject = raycastHit.transform.GetComponent<DrawableObject>();
            if (drawableObject != null) drawableObject.DrawAtPosition(raycastHit.textureCoord, drawingColor);
        }
    }
}