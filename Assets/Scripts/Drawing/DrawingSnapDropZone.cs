using System;
using UnityEngine;
using VRTK;

namespace Drawing
{
    [RequireComponent(typeof(SnapDropZonePatch))]
    public class DrawingSnapDropZone : MonoBehaviour
    {
        #region public fields

        public GameObject measurementHightlightObject;

        #endregion

        #region private fields

        private SnapDropZonePatch dropZonePatch;
        private VRTK_SnapDropZone vrtkSnapDropzone;
        private bool changedConvex;
        private GameObject currentlySnappedObject;

        #endregion
        
        private void Awake()
        {
            dropZonePatch = GetComponent<SnapDropZonePatch>();
            vrtkSnapDropzone = GetComponent<VRTK_SnapDropZone>();
            
            dropZonePatch.onObjectSuccesfullySnapped += OnObjectSnapped;
            dropZonePatch.onObjectSuccesfullyUnsnapped += OnObjectUnsnapped;

            vrtkSnapDropzone.ObjectExitedSnapDropZone += OnObjectExitedSnapDropZone;
        }
        
        private void OnDestroy()
        {
            dropZonePatch.onObjectSuccesfullySnapped -= OnObjectSnapped;
            dropZonePatch.onObjectSuccesfullySnapped -= OnObjectUnsnapped;
            
            vrtkSnapDropzone.ObjectExitedSnapDropZone -= OnObjectExitedSnapDropZone;
        }
        
        private void OnObjectSnapped()
        {
            //condition that checks if there is no other attached object in snapzone
            //OR if this is still the same attached object but exited the snapzone in the meantime
            if (currentlySnappedObject == null || (currentlySnappedObject == dropZonePatch.SnappedObject &&
                                                   currentlySnappedObject.GetComponent<DrawableObject>() &&
                                                   (!currentlySnappedObject.GetComponent<MeshCollider>() ||
                                                   currentlySnappedObject.GetComponent<MeshCollider>().convex)))
            {
                measurementHightlightObject.SetActive(false);

                if (currentlySnappedObject == null)
                {
                    Debug.Log("added drawing component");
                    dropZonePatch.SnappedObject.AddComponent<DrawableObject>();
                }

                currentlySnappedObject = dropZonePatch.SnappedObject;
                currentlySnappedObject.transform.SetParentUnscaled(transform);
                
                MeshCollider existentMeshCollider = currentlySnappedObject.GetComponent<MeshCollider>();
                if (existentMeshCollider)
                {
                    Debug.Log("hab schon meshcollider");

                    existentMeshCollider.convex = false;
                    changedConvex = true;
                }
                else
                {
                    Debug.Log("disabled collider");

                    if (currentlySnappedObject.GetComponent<Collider>()) 
                        currentlySnappedObject.GetComponent<Collider>().enabled = false;
                    currentlySnappedObject.AddComponent<MeshCollider>();
                }
            }
        }

        private void OnObjectUnsnapped()
        {
            if (currentlySnappedObject == null) return;
            
            DestroyImmediate(currentlySnappedObject.GetComponent<DrawableObject>());
            Debug.Log("destroyed drawable object component");
            measurementHightlightObject.SetActive(true);
            currentlySnappedObject = null;
        }

        private void OnObjectExitedSnapDropZone(object sender, SnapDropZoneEventArgs e)
        {
            if (currentlySnappedObject == null || currentlySnappedObject != e.snappedObject) return;
            if (!currentlySnappedObject.GetComponent<DrawableObject>()) return;
            
            if (changedConvex)
            {
                Debug.Log("changed to convex again");
                currentlySnappedObject.GetComponent<MeshCollider>().convex = true;
                changedConvex = false;
            }
            else
            {
                Debug.LogError("Fallbakc MeshCollider");
                MeshCollider fallbackMeshCollider = gameObject.AddComponent<MeshCollider>();
                fallbackMeshCollider.convex = true;
            }
        }
    }
}