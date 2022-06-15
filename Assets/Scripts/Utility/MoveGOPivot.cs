using UnityEngine;

namespace Utility
{
    public class MoveGOPivot : MonoBehaviour
    {
        [SerializeField]
        MeshFilter meshFilter;
    
        Mesh mesh;

        DamageFX dfx;

        [HideInInspector]
        public bool pivotWasCentered;

        // Start is called before the first frame update
        void Start()
        {
            if (meshFilter == null)  mesh = GetComponent<MeshFilter>().mesh;
            else mesh = meshFilter.mesh;

            dfx = GetComponent<DamageFX>();
        }

        public void MovePivotToCenter()
        {
            if (pivotWasCentered) return;
            pivotWasCentered = true;
            
        
            //calculate geometric center
            Vector3 geometricCenter = Vector3.zero;
            Vector3[] vertices = mesh.vertices;

            /*foreach (Vector3 vertex in vertices) {
                geometricCenter += vertex;
            }
            geometricCenter /= vertices.Length; */

            geometricCenter = mesh.bounds.center;

            //move each vertex, so that the geometric center is at 0|0
            for (int i=0; i<vertices.Length; i++)
            {
                vertices[i] -= geometricCenter;
            }
            mesh.vertices = vertices;

            //move pivot to old geometric center, so that object stays at the same position as before
            transform.localPosition += geometricCenter;


            //Damage FX effect moving
            Vector4[] dfxPoints = dfx.GetPoints();

            for (int i = 0; i < dfxPoints.Length; i++) {
                if (dfxPoints[i] != Vector4.zero)   
                {
                    //move points together with mesh
                    dfxPoints[i] = new Vector4(dfxPoints[i].x - geometricCenter.x, dfxPoints[i].y - geometricCenter.y, dfxPoints[i].z - geometricCenter.z, dfxPoints[i].w);
                }
                else {  //else all points have been processed
                    break;
                }
            }

            dfx.SetPoints(dfxPoints);


            //recalculate Mesh Collider
            foreach (Collider col in GetComponents<Collider>()) Destroy(col);

            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
    }
}
