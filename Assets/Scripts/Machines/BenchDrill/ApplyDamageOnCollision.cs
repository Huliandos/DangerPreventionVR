using System.Collections.Generic;
using UnityEngine;

public class ApplyDamageOnCollision : MonoBehaviour
{
    #region private fields

    List<GameObject> collidingObjects = new List<GameObject>();
    List<GameObject> collidingObjectsToIgnoreFromCutHand = new List<GameObject>();
    List<Vector3> collisionPoints = new List<Vector3>();
    List<bool> overObject = new List<bool>();

    #endregion

    #region serialized fields

    [SerializeField]
    float HitRadius = 0.1f, HitRadiusFlesh = 0.03f, DirtMetal = 1f, DirtFlesh = 1f, BurnMetal = 1f, BurnFlesh = 1f, HeatMetal = 1f, HeatFlesh = 1f, Clip = 0.7f;

    [SerializeField] GameObject metalPS, fleshPS;
    [SerializeField] float ImpactSize = 0.3f;

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision Entered w/ " + other.gameObject);
        if (other.gameObject.tag == Tags.splittable && !collidingObjects.Contains(other.gameObject) && !collidingObjectsToIgnoreFromCutHand.Contains(other.gameObject))
        {
            if (!other.gameObject.GetComponent<RootObject>())
            {
                metalPS.SetActive(true);
                Physics.IgnoreCollision(transform.parent.GetComponent<Collider>(), other.gameObject.GetComponent<Collider>());
            }
            else
            {
                if (fleshPS != null) fleshPS.SetActive(true);
            }

            //Debug.Log("if Entered w/ " + other.gameObject);
            //if (other.gameObject.GetComponent<RootObject>()) {
            //    if (!collidingObjects.Contains(other.gameObject.GetComponent<RootObject>().getRoot())) collidingObjects.Add(other.gameObject.GetComponent<RootObject>().getRoot());
            //}
            //else 
            collidingObjects.Add(other.gameObject);

            RaycastHit hit;
            Ray ray = new Ray(transform.position, -Vector3.up);
            other.gameObject.GetComponent<Collider>().Raycast(ray, out hit, 1);

            if (hit.collider != null) collisionPoints.Add(hit.point);
            else
            {
                ray = new Ray(transform.position, Vector3.up);
                other.gameObject.GetComponent<Collider>().Raycast(ray, out hit, 1);

                if (hit.collider != null) collisionPoints.Add(hit.point);
                else collisionPoints.Add(transform.position);
            }

            if (transform.position.y > other.gameObject.transform.position.y) overObject.Add(true);
            else overObject.Add(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collision Stay w/ " + other.gameObject);
        for (int i = 0; i < collidingObjects.Count; i++)
        {
            RaycastHit downwardsHit, upwardsHit;
            Ray ray = new Ray(transform.position, -Vector3.up);
            collidingObjects[i].GetComponent<Collider>().Raycast(ray, out downwardsHit, 1);

            ray = new Ray(transform.position, Vector3.up);
            collidingObjects[i].GetComponent<Collider>().Raycast(ray, out upwardsHit, 1);

            if (collisionPoints[i] != new Vector3(-9999, -9999, -9999) && transform.position.y > collisionPoints[i].y && overObject[i] == false)
            {
                //Debug.Log("Topdown drilling at Point X: " + collisionPoints[i].x + " Y: " + collisionPoints[i].y + " Z: " + collisionPoints[i].z);
                //Entry Point Hole

                //TODO: Yeeet Drill Bit here -> check for state
                if (gameObject.GetComponentInParent<DrillBitController>().State == DrillBitController.DrillBitState.IMPROPERLY_SNAPPED)
                {
                    Debug.Log("Launching");
                    //TODO: DrillBitController Invoke("LaunchDrillBit")
                    GetComponentInParent<DrillBitController>().LaunchDrillBit();
                }
                    
                    
                DecideTypeOfDamage(collidingObjects[i], collisionPoints[i]);

                collisionPoints[i] = new Vector3(-9999, -9999, -9999);
                
                overObject[i] = true;
            }
            else if (collisionPoints[i] != new Vector3(-9999, -9999, -9999) && transform.position.y < collisionPoints[i].y && overObject[i] == true)
            {
                //Debug.Log("Bottomup drilling at Point X: " + collisionPoints[i].x + " Y: " + collisionPoints[i].y + " Z: " + collisionPoints[i].z);
                //Entry Point Hole

                //TODO: Yeeet Drill Bit here -> check for state
                if (gameObject.GetComponentInParent<DrillBitController>().State == DrillBitController.DrillBitState.IMPROPERLY_SNAPPED)
                {
                    //TODO: DrillBitController Invoke("LaunchDrillBit")
                    GetComponentInParent<DrillBitController>().LaunchDrillBit();
                }

                DecideTypeOfDamage(collidingObjects[i], collisionPoints[i]);

                collisionPoints[i] = transform.position;

                overObject[i] = false;
            }
            else if (downwardsHit.collider != null)
            {
                collisionPoints[i] = downwardsHit.point;
            }
            else if (upwardsHit.collider != null)
            {
                collisionPoints[i] = upwardsHit.point;
            }
            else
            {
                collisionPoints[i] = new Vector3(-9999, -9999, -9999);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Collision Exited w/ " + other.gameObject);
        //if (other.gameObject.GetComponent<RootObject>())
        //{
        //    if (collidingObjects.Contains(other.gameObject.GetComponent<RootObject>().getRoot())) collidingObjects.Add(other.gameObject.GetComponent<RootObject>().getRoot());
        //}
        //else collidingObjects.Remove(other.gameObject);
        if (collidingObjects.Contains(other.gameObject))
        {
            if (!other.gameObject.GetComponent<RootObject>())
            {
                Physics.IgnoreCollision(transform.parent.GetComponent<Collider>(), other.gameObject.GetComponent<Collider>(), false);
                metalPS.SetActive(false);
            }
            else
            {
                if (fleshPS != null) fleshPS.SetActive(false);
            }

            int objectIndex = collidingObjects.IndexOf(other.gameObject);
            collidingObjects.Remove(other.gameObject);

            collisionPoints.RemoveAt(objectIndex);
            overObject.RemoveAt(objectIndex);
        }
    }

    #region commented out

    //old way that uses
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == Tags.splittable) {
            //if we are colliding with the player Hand
            if (other.gameObject.GetComponent<RootObject>())
            {
                GameObject go = duplicateSkinnedMesh(other.gameObject);

                go.AddComponent<DamageFX>();
                MeshCollider col = go.AddComponent<MeshCollider>();
                col.convex = true;
                col.sharedMesh = go.GetComponent<MeshFilter>().mesh;
                go.AddComponent<IsFlesh>();

                Vector3 surfaceCollisionPoint = findSurfaceCollisionPoint(go);
                GetComponent<TestBloodInstantiation>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint + transform.up);

                //The effect looks better when doing it 5 times won collision enter and 7 times on collision exit
                //TODO: Find a better way to do this later. Problem ist, that the hands MeshCollider doesn't represent its Mesh surface
                //We probably have to generate Compound Collider in runtime to make this whole endavour work
                for(int i=0; i<5; i++) DealDamage(go, surfaceCollisionPoint, true);
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().gameOver(1);

                if (fleshPS != null)
                {
                    var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                    fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
                }
                //Destroy skinned hand
                Destroy(other.gameObject.GetComponent<RootObject>().getRoot());
            }
            else
            {
                if (other.gameObject.GetComponent<IsFlesh>() != null)
                {
                    Vector3 surfaceCollisionPoint = findSurfaceCollisionPoint(other.gameObject);
                    GetComponent<TestBloodInstantiation>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint + transform.up);

                    for (int i = 0; i < 5; i++) DealDamage(other.gameObject, surfaceCollisionPoint, true);
                    //DealDamage(collision.gameObject, collision.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position), true);

                    if (fleshPS != null)
                    {
                        var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                        fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
                    }
                }
                else
                {
                    //tries to fetch a point on the surface of the colliding object
                    //only works for cubes!
                    //DealDamage(collision.gameObject, collision.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position));
                    DealDamage(other.gameObject, other.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position));
                    //DealDamage(collision.gameObject, transform.position);

                    if (!metalPS) return;
                    var fx = Instantiate(metalPS, transform.position, Quaternion.LookRotation(transform.up));
                    fx.transform.localScale = Vector3.one * HitRadius + Vector3.one * ImpactSize;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == Tags.splittable)
        {
            //if we are colliding with the player Hand
            if (other.gameObject.GetComponent<RootObject>())
            {
                GameObject go = duplicateSkinnedMesh(other.gameObject);

                go.AddComponent<DamageFX>();
                MeshCollider col = go.AddComponent<MeshCollider>();
                col.convex = true;
                col.sharedMesh = go.GetComponent<MeshFilter>().mesh;
                go.AddComponent<IsFlesh>();

                Vector3 surfaceCollisionPoint = findSurfaceCollisionPoint(go);
                GetComponent<TestBloodInstantiation>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint + transform.up);

                for (int i = 0; i < 7; i++) DealDamage(go, surfaceCollisionPoint, true);
                //DealDamage(go, go.GetComponent<Renderer>().bounds.ClosestPoint(transform.position), true);

                if (fleshPS != null)
                {
                    var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                    fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
                }
                //Destroy skinned hand
                Destroy(other.gameObject.GetComponent<RootObject>().getRoot());
            }
            else
            {
                if (other.gameObject.GetComponent<IsFlesh>() != null)
                {
                    Vector3 surfaceCollisionPoint = findSurfaceCollisionPoint(other.gameObject);
                    GetComponent<TestBloodInstantiation>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint - transform.up);

                    for (int i = 0; i < 7; i++) DealDamage(other.gameObject, surfaceCollisionPoint, true);
                    //DealDamage(collision.gameObject, collision.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position), true);

                    if (fleshPS != null)
                    {
                        var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                        fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
                    }
                }
                else
                {
                    //tries to fetch a point on the surface of the colliding object
                    //only works for cubes!
                    DealDamage(other.gameObject, other.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position));
                    //DealDamage(collision.gameObject, transform.position);

                    if (!metalPS) return;
                    var fx = Instantiate(metalPS, transform.position, Quaternion.LookRotation(transform.up));
                    fx.transform.localScale = Vector3.one * HitRadius + Vector3.one * ImpactSize;
                }
            }
        }
    }
    */

    #endregion

    void DecideTypeOfDamage(GameObject goToDrill, Vector3 surfaceCollisionPoint)
    {
        //if we are colliding with the player Hand
        if (goToDrill.GetComponent<RootObject>())
        {
            GameObject go = DuplicateSkinnedMesh(goToDrill);

            //collidingObjects[collidingObjects.IndexOf(goToDrill)] = go;

            //remove all references to the players Hand Colliders

            List<GameObject> gosOfHand = new List<GameObject>();
            foreach (GameObject gameO in collidingObjects)
            {
                //if (gameO.GetComponent<RootObject>() && goToDrill.GetComponent<RootObject>().getRoot() == gameO.GetComponent<RootObject>().getRoot())
                if (gameO.GetComponent<RootObject>() && goToDrill.GetComponent<RootObject>().getMeshRenderer().gameObject == gameO.GetComponent<RootObject>().getMeshRenderer().gameObject)
                {
                    gosOfHand.Add(gameO);
                }
            }
            if (gosOfHand.Count > 0){
                foreach (RootObject rootObject in gosOfHand[0].GetComponent<RootObject>().getRoot().GetComponentsInChildren<RootObject>()) {
                    collidingObjectsToIgnoreFromCutHand.Add(rootObject.gameObject);
                }
            }
            foreach (GameObject gameO in gosOfHand)
            {
                collidingObjects.Remove(gameO);
            }
            

            collidingObjects.Add(go);

            go.AddComponent<DamageFX>();
            MeshCollider col = go.AddComponent<MeshCollider>();
            col.convex = true;
            col.sharedMesh = go.GetComponent<MeshFilter>().mesh;
            go.AddComponent<IsFlesh>();

            GetComponent<SpawnBlood>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint + transform.up);

            //The effect looks better when doing it 5 times won collision enter and 7 times on collision exit
            //TODO: Find a better way to do this later. Problem is, that the hands MeshCollider doesn't represent its Mesh surface
            //We probably have to generate Compound Collider in runtime to make this whole endavour work
            for (int i = 0; i < 5; i++) DealDamage(go, surfaceCollisionPoint, true);

            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(Injury.Drilling);

            if (fleshPS != null)
            {
                var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
            }
            //Destroy skinned hand
            //Destroy(goToDrill.GetComponent<RootObject>().getRoot());
            Destroy(goToDrill.GetComponent<RootObject>().getMeshRenderer().gameObject);
        }
        else
        {
            if (goToDrill.GetComponent<IsFlesh>() != null)
            {
                GetComponent<SpawnBlood>().Bleed(surfaceCollisionPoint, surfaceCollisionPoint + transform.up);

                //this for loop is here to secure, that you can see a proper hole from drilling, as the shader can be unreliable sometimes
                for (int i = 0; i < 5; i++) DealDamage(goToDrill, surfaceCollisionPoint, true);
                //DealDamage(collision.gameObject, collision.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position), true);

                if (fleshPS != null)
                {
                    var fx = Instantiate(fleshPS, transform.position, Quaternion.LookRotation(transform.up));
                    fx.transform.localScale = Vector3.one * HitRadiusFlesh + Vector3.one * ImpactSize;
                }
            }
            else
            {
                //DealDamage(collision.gameObject, collision.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(transform.position));

                //DealDamage(goToDrill, goToDrill.GetComponent<Renderer>().bounds.ClosestPoint(transform.position));

                //this for loop is here to secure, that you can see a proper hole from drilling, as the shader can be unreliable sometimes
                for (int i = 0; i < 2; i++) DealDamage(goToDrill, surfaceCollisionPoint);

            }
        }
    }

    //Vector3 FindSurfaceCollisionPoint(GameObject go)
    //{
    //    RaycastHit hit;
    //    Ray ray;

    //    if (go.transform.position.y < transform.position.y) ray = new Ray(transform.position, -transform.up);   //if object to drill is beneath the drillBit
    //    else ray = new Ray(transform.position, transform.up);

    //    go.GetComponent<Collider>().Raycast(ray, out hit, 2);

    //    if (hit.collider) return hit.point;

    //    //default return
    //    return transform.position;
    //}

    #region duplicate skinned mesh, so that drilling actually works on it
    GameObject DuplicateSkinnedMesh(GameObject collidingObject)
    {
        //GameObject gameObjectToDrill = collidingObject.GetComponent<RootObject>().getRoot();
        GameObject gameObjectToDrill = collidingObject.GetComponent<RootObject>().getMeshRenderer().gameObject;

        Mesh testMesh = new Mesh();
        gameObjectToDrill.GetComponentInChildren<SkinnedMeshRenderer>().BakeMesh(testMesh);

        //reverse Mesh normals if Object is negatively scaled
        if (gameObjectToDrill.transform.localScale.x < 0 || gameObjectToDrill.transform.localScale.y < 0 || gameObjectToDrill.transform.localScale.z < 0)
        {
            List<int[]> submeshVertexIndices = new List<int[]>();
            for (int i = 0; i < testMesh.subMeshCount; i++)
            {
                submeshVertexIndices.Add(testMesh.GetTriangles(i));
            }

            for (int i = 0; i < submeshVertexIndices.Count; i++)
            {
                for (int j = 0; j < submeshVertexIndices[i].Length; j += 3)
                {
                    int storedIndex = submeshVertexIndices[i][j];
                    submeshVertexIndices[i][j] = submeshVertexIndices[i][j + 2];
                    submeshVertexIndices[i][j + 2] = storedIndex;
                }
            }

            for (int i = 0; i < testMesh.subMeshCount; i++)
            {
                testMesh.SetTriangles(submeshVertexIndices[i], i);
            }
        }

        //create new GameObject an add necessary components
        GameObject go = new GameObject();
        go.name = gameObjectToDrill.name + "_Drilled";
        //go.tag = gameObjectToCut.tag;
        go.tag = "Splittable";
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.AddComponent<SplittableInfo>();

        //copy splittable Info
        go.GetComponent<SplittableInfo>().setCap(gameObjectToDrill.GetComponent<SplittableInfo>().getCap());
        go.GetComponent<SplittableInfo>().setConcave(gameObjectToDrill.GetComponent<SplittableInfo>().getConcave());
        go.GetComponent<SplittableInfo>().setCutMaterial(gameObjectToDrill.GetComponent<SplittableInfo>().getCutMaterial());
        go.GetComponent<SplittableInfo>().setMeshSeperation(gameObjectToDrill.GetComponent<SplittableInfo>().getMeshSeperation());

        //set Mesh
        go.GetComponent<MeshFilter>().mesh = testMesh;
        go.GetComponent<MeshRenderer>().material = gameObjectToDrill.GetComponentInChildren<SkinnedMeshRenderer>().material;

        //Vector3 offsetPosition = gameObjectToCut.transform.localPosition + gameObjectToCut.GetComponent<SkinnedMeshOffset>().getPositionOffset();
        //Vector3 offsetEulerAngles = gameObjectToCut.GetComponent<SkinnedMeshOffset>().getEulerAngleOffset();
        Vector3 offsetPosition = gameObjectToDrill.transform.parent.localPosition + gameObjectToDrill.transform.parent.GetComponent<SkinnedMeshOffset>().PositionOffset;
        Vector3 offsetEulerAngles = gameObjectToDrill.transform.parent.GetComponent<SkinnedMeshOffset>().EulerAngleOffset;

        //set position
        //go.transform.position = gameObjectToCut.transform.position + gameObjectToCut.GetComponent<SkinnedMeshOffset>().getPositionOffset();
        go.transform.position = gameObjectToDrill.GetComponentInChildren<SkinnedMeshRenderer>().transform.position;

        go.transform.eulerAngles = gameObjectToDrill.GetComponentInChildren<SkinnedMeshRenderer>().transform.eulerAngles;

        Vector3 absoluteScale = new Vector3(Mathf.Abs(gameObjectToDrill.transform.localScale.x), Mathf.Abs(gameObjectToDrill.transform.localScale.y), Mathf.Abs(gameObjectToDrill.transform.localScale.z));
        //go.transform.localScale = absoluteScale - gameObjectToCut.transform.parent.GetComponent<SkinnedMeshOffset>().getScaleOffset();
        go.transform.localScale -= gameObjectToDrill.transform.parent.GetComponent<SkinnedMeshOffset>().ScaleOffset;

        go.transform.parent = gameObjectToDrill.transform.parent;

        return go;
    }
    #endregion

    private void DealDamage(GameObject goToDealDamageTo, Vector3 damagePoint, bool flesh = false)
    {
        DamageFX dfx = goToDealDamageTo.GetComponent<DamageFX>();
        if (dfx != null)
        {
            if (flesh) dfx.Hit(dfx.transform.InverseTransformPoint(damagePoint), HitRadiusFlesh, DirtFlesh, BurnFlesh, HeatFlesh, Clip);
            else dfx.Hit(dfx.transform.InverseTransformPoint(damagePoint), HitRadius, DirtMetal, BurnMetal, HeatMetal, Clip);
        }
    }

    /// <summary>
    /// Call this, when the tips collider gets disabled, so old collision references get removed
    /// </summary>
    public void ClearCollidingObjects() {
        if (collidingObjects.Count > 0) { 
            collidingObjects = new List<GameObject>(); 
            metalPS.SetActive(false);
        }
    }
}
