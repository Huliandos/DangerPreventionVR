using UnityEngine;
using System.Collections;

public class realtimeBooleanDrill : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

    private void Update()
    {
        
    }

    private void OnCollisionEnter(Collision col)
	{
		Debug.Log("Collision entered w/ " + col.gameObject);

		//BooleanMeshEdit booleanMesh = new BooleanMeshEdit(GetComponent<MeshCollider>(), col.gameObject.GetComponent<MeshCollider>());
		BooleanMeshEdit booleanMesh = new BooleanMeshEdit(gameObject, col.gameObject);
		col.gameObject.GetComponent<MeshFilter>().mesh = booleanMesh.Difference();
	}
}
