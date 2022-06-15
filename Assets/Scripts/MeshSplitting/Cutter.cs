using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utility;

public class Cutter
{
    /// <summary>
    /// This class is the heart of mesh cutting, holding all it's operations and logic
    /// Logic inclues
    /// - Triangle sorting
    /// - Triangle splitting
    /// - Hole Filling
    /// - Mesh and GameObject creation
    /// - Mesh seperation
    /// 
    /// The Behavior for skinned Mesh's is, that it bakes the Mesh in its current situation
    /// and then but the normal Mesh
    /// Skinned Mesh cutting seems to be an obnoxious issue for now, so developing an algorithm for that is due for later
    /// </summary>

    #region private fields

    //Variables hold reference to different Data of the mesh to cut for easy access accross the class
    GameObject gameObjectToCut, skinnedMeshRootObject;
    Mesh meshToCut;
    volatile Material cutMaterial;
    bool cap;

    //multi threading
    object lockGeneratedMesh = new object();

    bool negativelyScaledSkinnedMesh;
    //float timeSpend;
    //int valueNumber;

    // variables to save on main Thread, so the cutting process can be offloaded to another thread
    Vector3 originialSkinnedMeshLocalPosition;
    Vector3 localUp;
    Plane plane;
    Renderer renderer;
    Vector3[] meshVertices;
    Vector3[] meshNormals;
    Vector2[] meshUvs;
    Vector2[] meshRtLightmapUvs; //this are the global illumination map UVs
    List<int[]> submeshVertexIndices = new List<int[]>();
    Vector3 position, localPosition;
    Quaternion rotation;

    #endregion

    public void InitCutOnOtherThread(GameObject gameObjectToCut, Vector3 contactPoint, Vector3 up, Material cutMaterial = null, bool cap = true, bool concave = true, bool meshSeperation = false, bool skinnedMesh = false)
    {
        #region skinned Mesh duplication
        //this is for determining which half of the Mesh to actually cut off
        originialSkinnedMeshLocalPosition = gameObjectToCut.transform.localPosition;

        if (skinnedMesh)
        {
            //copy Mesh Data
            Mesh testMesh = new Mesh();
            gameObjectToCut.GetComponent<SkinnedMeshRenderer>().BakeMesh(testMesh);

            //for left hand
            if (gameObjectToCut.transform.parent.localScale.x < 0 || gameObjectToCut.transform.parent.localScale.y < 0 || gameObjectToCut.transform.parent.localScale.z < 0)
            {
                negativelyScaledSkinnedMesh = true;
            }

            //create new GameObject an add necessary components
            GameObject go = new GameObject();
            go.name = gameObjectToCut.name + "_Cut";
            //go.tag = gameObjectToCut.tag;
            go.tag = "Splittable";
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<SplittableInfo>();

            //copy splittable Info
            go.GetComponent<SplittableInfo>().setCap(gameObjectToCut.GetComponent<SplittableInfo>().getCap());
            go.GetComponent<SplittableInfo>().setConcave(gameObjectToCut.GetComponent<SplittableInfo>().getConcave());
            go.GetComponent<SplittableInfo>().setCutMaterial(gameObjectToCut.GetComponent<SplittableInfo>().getCutMaterial());
            go.GetComponent<SplittableInfo>().setMeshSeperation(gameObjectToCut.GetComponent<SplittableInfo>().getMeshSeperation());

            //set Mesh
            go.GetComponent<MeshFilter>().mesh = testMesh;
            go.GetComponent<MeshRenderer>().material = gameObjectToCut.GetComponent<SkinnedMeshRenderer>().material;

            //Vector3 offsetPosition = gameObjectToCut.transform.localPosition + gameObjectToCut.GetComponent<SkinnedMeshOffset>().getPositionOffset();
            //Vector3 offsetEulerAngles = gameObjectToCut.GetComponent<SkinnedMeshOffset>().getEulerAngleOffset();
            Vector3 offsetPosition = gameObjectToCut.transform.parent.localPosition + gameObjectToCut.transform.parent.GetComponent<SkinnedMeshOffset>().PositionOffset;
            Vector3 offsetEulerAngles = gameObjectToCut.transform.parent.GetComponent<SkinnedMeshOffset>().EulerAngleOffset;

            //set position
            //go.transform.position = gameObjectToCut.transform.position + gameObjectToCut.GetComponent<SkinnedMeshOffset>().getPositionOffset();
            go.transform.position = gameObjectToCut.GetComponentInChildren<SkinnedMeshRenderer>().transform.position;

            go.transform.eulerAngles = gameObjectToCut.GetComponentInChildren<SkinnedMeshRenderer>().transform.eulerAngles;

            Vector3 absoluteScale = new Vector3(Mathf.Abs(gameObjectToCut.transform.localScale.x), Mathf.Abs(gameObjectToCut.transform.localScale.y), Mathf.Abs(gameObjectToCut.transform.localScale.z));
            //go.transform.localScale = absoluteScale - gameObjectToCut.transform.parent.GetComponent<SkinnedMeshOffset>().getScaleOffset();
            go.transform.localScale -= gameObjectToCut.transform.parent.GetComponent<SkinnedMeshOffset>().ScaleOffset;

            go.transform.parent = gameObjectToCut.transform.parent;

            //--> Enable to showcase hand cutting. This is buggy rn
            //contactPoint = contactPoint + gameObjectToCut.GetComponent<SkinnedMeshOffset>().getPositionOffset();

            //GameObject.Destroy(gameObjectToCut);
            skinnedMeshRootObject = gameObjectToCut;

            gameObjectToCut = go;
        }
        #endregion

        #region cut initializer
        //Adapting plane normal to the new matrix generated by the local size of the gameObject to cut
        localUp = new Vector3(gameObjectToCut.transform.InverseTransformDirection(up).x * gameObjectToCut.transform.localScale.x,
            gameObjectToCut.transform.InverseTransformDirection(up).y * gameObjectToCut.transform.localScale.y,   //idea to fix scaling problem
            gameObjectToCut.transform.InverseTransformDirection(up).z * gameObjectToCut.transform.localScale.z);

        //Plane plane = new Plane(localUp, gameObjectToCut.transform.InverseTransformDirection(contactPoint));
        plane = new Plane(localUp, gameObjectToCut.transform.InverseTransformPoint(contactPoint));
        //Plane plane = new Plane(localUp, contactPoint);

        this.gameObjectToCut = gameObjectToCut;

        meshToCut = gameObjectToCut.GetComponent<MeshFilter>().mesh;

        //if (cutMaterial != null) this.cutMaterial = new Material(cutMaterial);
        if (cutMaterial != null) this.cutMaterial = cutMaterial;
        else this.cutMaterial = null;
        this.cap = cap;

        meshVertices = meshToCut.vertices;
        meshNormals = meshToCut.normals;
        meshUvs = meshToCut.uv;
        meshRtLightmapUvs = meshToCut.uv2; //this are the global illumination map UVs

        for (int i = 0; i < meshToCut.subMeshCount; i++)
        {
            submeshVertexIndices.Add(meshToCut.GetTriangles(i));
        }
        #endregion

        renderer = gameObjectToCut.GetComponent<Renderer>();

        #region Mesh initializer
        position = gameObjectToCut.transform.position;
        localPosition = gameObjectToCut.transform.localPosition;
        rotation = gameObjectToCut.transform.rotation;
        #endregion

        GeneratedMesh positiveSideMesh = new GeneratedMesh();
        GeneratedMesh negativeSideMesh = new GeneratedMesh();

        StartAsyncCut(cap, concave, meshSeperation, skinnedMesh, positiveSideMesh, negativeSideMesh);

        //Thread thread = new Thread(() => cut(gameObjectToCut, contactPoint, up, cutMaterial, cap, concave, meshSeperation, skinnedMesh, positiveSideMesh, negativeSideMesh));
        //thread.Start();
    }

    async void StartAsyncCut(bool cap, bool concave, bool meshSeperation, bool skinnedMesh, GeneratedMesh positiveSideMesh, GeneratedMesh negativeSideMesh)
    {

        //You can't pass Materials to asynchronous functions, so we pass strings to compare and add the materials to each cut half
        string[] rendererMaterialNames = new string[renderer.materials.Length];
        for (int i = 0; i < rendererMaterialNames.Length; i++)
        {
            rendererMaterialNames[i] = renderer.materials[i].name;
        }

        string cutMaterialName;
        if (cutMaterial != null) cutMaterialName = cutMaterial.name;
        else cutMaterialName = "";

        //Cut asynchronus
        await Task.Run(() => Cut(cap, positiveSideMesh, negativeSideMesh, cutMaterialName, rendererMaterialNames));

        //create object as soon as cutting is done
        InstantiateMesh(concave, meshSeperation, positiveSideMesh, negativeSideMesh, skinnedMesh);
    }

    void InstantiateMesh(bool concave, bool meshSeperation, GeneratedMesh positiveSideMesh, GeneratedMesh negativeSideMesh, bool skinnedMesh)
    {
        //Concave Meshes require special Operations like finding seperated Meshes, in case that they got cut into more than two pieces

        if (concave && meshSeperation)
        {
            //this doesn't work for skinned meshs yet
            if (skinnedMesh)
            {
                GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(Injury.Cutting);

                Vector3 positiveGeometricCenter = Vector3.zero, negativeGeometricCenter = Vector3.zero;

                foreach (Vector3 vertex in positiveSideMesh.getVertices()) positiveGeometricCenter += vertex;
                foreach (Vector3 vertex in negativeSideMesh.getVertices()) negativeGeometricCenter += vertex;

                positiveGeometricCenter /= positiveSideMesh.getVertices().Count;
                negativeGeometricCenter /= negativeSideMesh.getVertices().Count;

                positiveGeometricCenter = gameObjectToCut.transform.TransformPoint(positiveGeometricCenter);
                negativeGeometricCenter = gameObjectToCut.transform.TransformPoint(negativeGeometricCenter);

                //Debug.Log("Positive geometric center: " + positiveGeometricCenter);
                //Debug.Log("Negative geometric center: " + negativeGeometricCenter);
                //Debug.Log("Positive geometric center: " + gameObjectToCut.transform.TransformPoint(positiveGeometricCenter));
                //Debug.Log("Negative geometric center: " + gameObjectToCut.transform.TransformPoint(negativeGeometricCenter));

                if ((positiveGeometricCenter - gameObjectToCut.transform.parent.position).magnitude <
                        (negativeGeometricCenter - gameObjectToCut.transform.parent.position).magnitude)
                {
                    GameObject connectedHalf = CreateGameObject(positiveSideMesh);
                    GameObject disconnectedHalf = CreateGameObject(negativeSideMesh);

                    if (connectedHalf && disconnectedHalf)
                    {
                        //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(0);

                        //this is the players hand
                        SpawnBlood testBloodInstantiation = connectedHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(negativeGeometricCenter - connectedHalf.GetComponent<Renderer>().bounds.center);

                        disconnectedHalf.transform.parent = null;
                        disconnectedHalf.AddComponent<Rigidbody>();
                        disconnectedHalf.GetComponent<Rigidbody>().useGravity = true;

                        testBloodInstantiation = disconnectedHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(positiveGeometricCenter - disconnectedHalf.GetComponent<Renderer>().bounds.center);

                        StartAsyncMeshSeperation(disconnectedHalf);
                    }
                }
                else
                {
                    GameObject connectedHalf = CreateGameObject(negativeSideMesh);
                    GameObject disconnectedHalf = CreateGameObject(positiveSideMesh);
                    if(connectedHalf && disconnectedHalf)
                    {
                        //GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(0);

                        //this is the players hand
                        SpawnBlood testBloodInstantiation = connectedHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(positiveGeometricCenter - connectedHalf.GetComponent<Renderer>().bounds.center);

                        disconnectedHalf.transform.parent = null;
                        disconnectedHalf.AddComponent<Rigidbody>();
                        disconnectedHalf.GetComponent<Rigidbody>().useGravity = true;

                        testBloodInstantiation = disconnectedHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(negativeGeometricCenter - disconnectedHalf.GetComponent<Renderer>().bounds.center);

                        StartAsyncMeshSeperation(disconnectedHalf);
                    }
                }
            }
            else
            {
                FindSeperatedMeshes(positiveSideMesh);
                FindSeperatedMeshes(negativeSideMesh);
            }
        }
        else
        {
            if (skinnedMesh)
            {
                GameObject firstHalf = CreateGameObject(positiveSideMesh);
                GameObject secondHalf = CreateGameObject(negativeSideMesh);

                if (firstHalf != null && secondHalf != null)
                {
                    GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().GameOver(Injury.Cutting);

                    //Debug.Log(firstHalf.GetComponent<Renderer>().bounds.center);
                    //Debug.Log(firstHalf.transform.InverseTransformPoint(firstHalf.GetComponent<Renderer>().bounds.center));

                    //if Mesh of first half is closer to origin objects position
                    if ((firstHalf.GetComponent<Renderer>().bounds.center - firstHalf.transform.parent.position).magnitude <
                        (secondHalf.GetComponent<Renderer>().bounds.center - secondHalf.transform.parent.position).magnitude)
                    {
                        secondHalf.transform.parent = null;
                        secondHalf.AddComponent<Rigidbody>();
                        secondHalf.GetComponent<Rigidbody>().useGravity = true;

                        //this is the players hand
                        SpawnBlood testBloodInstantiation = firstHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(secondHalf.GetComponent<Renderer>().bounds.center- firstHalf.GetComponent<Renderer>().bounds.center);

                        testBloodInstantiation = secondHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(firstHalf.GetComponent<Renderer>().bounds.center- secondHalf.GetComponent<Renderer>().bounds.center);
                    }
                    else
                    {
                        firstHalf.transform.parent = null;
                        firstHalf.AddComponent<Rigidbody>();
                        firstHalf.GetComponent<Rigidbody>().useGravity = true;

                        SpawnBlood testBloodInstantiation = firstHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(secondHalf.GetComponent<Renderer>().bounds.center- firstHalf.GetComponent<Renderer>().bounds.center);

                        //this is the players hand
                        testBloodInstantiation = secondHalf.AddComponent<SpawnBlood>();
                        testBloodInstantiation.InvokeBleedFromCutterClass(firstHalf.GetComponent<Renderer>().bounds.center - secondHalf.GetComponent<Renderer>().bounds.center);
                    }
                }
            }
            else
            {
                //disables the VRTK_Interactable_Object Script on both halfs so that they falls down and don't stick to the players hands
                GameObject firstHalf = CreateGameObject(positiveSideMesh);
                GameObject secondHalf = CreateGameObject(negativeSideMesh);

                GameObject grabbingObject = gameObjectToCut.GetComponent<InteractableObjectPatch>().grabbingObject;

                if (grabbingObject != null && firstHalf.GetComponent<InteractableObjectPatch>() != null && firstHalf != null && secondHalf != null)
                {
                    if ((firstHalf.GetComponent<Renderer>().bounds.center - grabbingObject.transform.position).magnitude <
                        (secondHalf.GetComponent<Renderer>().bounds.center - grabbingObject.transform.position).magnitude)
                    {
                        firstHalf.GetComponent<InteractableObjectPatch>().setupNewGrabbingReference();
                        secondHalf.GetComponent<InteractableObjectPatch>().ForceStopInteracting();
                    }
                    else
                    {
                        firstHalf.GetComponent<InteractableObjectPatch>().ForceStopInteracting();
                        secondHalf.GetComponent<InteractableObjectPatch>().setupNewGrabbingReference();
                    }

                }
            }
        }

        //Destroy the root gameObject after cutting and object creation has been done
        if (skinnedMeshRootObject != null) GameObject.Destroy(skinnedMeshRootObject);
        GameObject.Destroy(gameObjectToCut);
    }

    #region Mesh Cutting procedures
    //Function that gets called from outer classes. Initializes all Variables and works through the procedure of Mesh cutting
    public async void Cut(bool cap, GeneratedMesh positiveSideMesh, GeneratedMesh negativeSideMesh, string cutMaterialName, string[] rendererMaterialNames)
    {

        List<Vector3> addedVertices = new List<Vector3>();              //List of newly added vertices. It is used to cap the mesh later

        List<Thread> threads = new List<Thread>();

        //float time = Time.realtimeSinceStartup;
        //parsing each submesh layer seperately

        if (negativelyScaledSkinnedMesh)
        {
            //flip all triangles in the Mesh
            for (int i = 0; i < submeshVertexIndices.Count; i++)
            {
                for (int j = 0; j < submeshVertexIndices[i].Length; j += 3)
                {
                    int storedIndex = submeshVertexIndices[i][j];
                    submeshVertexIndices[i][j] = submeshVertexIndices[i][j + 2];
                    submeshVertexIndices[i][j + 2] = storedIndex;
                }
            }
        }

        for (int i = 0; i < submeshVertexIndices.Count; i++)
        {
            //going through each Triangle within the submesh
            for (int j = 0; j < submeshVertexIndices[i].Length; j += 3)
            {
                int startTriangleIndex = j; // these three lines of code are here, to prevent an error, where all variables get accessed like reference type variables in C# Multi THreading
                int submeshIndex = i;
                int[] submeshVertexIndicesToPass = submeshVertexIndices[i];

                Thread thread = new Thread(() => SortTriangle(submeshVertexIndicesToPass, startTriangleIndex, submeshIndex, plane, addedVertices, positiveSideMesh, negativeSideMesh
                    , meshVertices, meshNormals, meshUvs, meshRtLightmapUvs));
                threads.Add(thread);
                thread.Start();
            }
        }

        foreach (Thread thread in threads) thread.Join();

        if (cap)
        {
            FillCut(addedVertices, plane, positiveSideMesh, negativeSideMesh, cutMaterialName, rendererMaterialNames);
        }


        //float time = Time.realtimeSinceStartup;
        //timeSpend += Time.realtimeSinceStartup - time;
        //valueNumber++;

        //Debug.Log("Time spend: " + (Time.realtimeSinceStartup - time));
        //Debug.Log("average value: " + timeSpend/valueNumber);
    }

    //Multithread vertex sorting
    void SortTriangle(int[] submeshVertexIndices, int startTriangleIndex, int submeshIndex, Plane plane, List<Vector3> addedVertices,
        GeneratedMesh positiveSideMesh, GeneratedMesh negativeSideMesh, Vector3[] meshVertices, Vector3[] meshNormals, Vector2[] meshUvs, Vector2[] meshRtLightmapUvs)
    {
        int[] triangleVertexIndices = new int[3];

        for (int k = 0; k < 3; k++)
            triangleVertexIndices[k] = submeshVertexIndices[startTriangleIndex + k];

        //get current Triangle
        Vector3[] vertices = new Vector3[3];
        Vector3[] normals = new Vector3[3];
        Vector2[] uvs = new Vector2[3];
        Vector2[] rtLightmapUvs = new Vector2[3];

        for (int i = 0; i < 3; i++)
        {
            vertices[i] = meshVertices[triangleVertexIndices[i]];
            normals[i] = meshNormals[triangleVertexIndices[i]];
            uvs[i] = meshUvs[triangleVertexIndices[i]];
            if (meshRtLightmapUvs.Length > 0)
                rtLightmapUvs[i] = meshRtLightmapUvs[triangleVertexIndices[i]];
            else
                rtLightmapUvs[i] = new Vector2(0, 0);               //--> Quickfix. Todo: Fix properly later
        }

        MeshTriangle currentTriangle = new MeshTriangle(vertices, normals, uvs, rtLightmapUvs, submeshIndex);

        bool[] isVertexPositiveSide = new bool[3];

        //Triangle sorting
        isVertexPositiveSide[0] = plane.GetSide(meshVertices[triangleVertexIndices[0]]);
        isVertexPositiveSide[1] = plane.GetSide(meshVertices[triangleVertexIndices[1]]);
        isVertexPositiveSide[2] = plane.GetSide(meshVertices[triangleVertexIndices[2]]);

        lock (lockGeneratedMesh)
        {
            if (isVertexPositiveSide[0] && isVertexPositiveSide[1] && isVertexPositiveSide[2])
                positiveSideMesh.addTriangle(currentTriangle);
            else if (!isVertexPositiveSide[0] && !isVertexPositiveSide[1] && !isVertexPositiveSide[2])
                negativeSideMesh.addTriangle(currentTriangle);
            else
                CutTriangle(plane, currentTriangle, isVertexPositiveSide, positiveSideMesh, negativeSideMesh, addedVertices);
        }
    }

    //Function that turns an array of vertex indices into the corrosponding Mesh Triangle, populating it with its values
    MeshTriangle GetTriangle(int[] triangleVertexIndices, int submeshIndex, Mesh meshToCut)
    {
        Vector3[] vertices = new Vector3[3];
        Vector3[] normals = new Vector3[3];
        Vector2[] uvs = new Vector2[3];
        Vector2[] rtLightmapUvs = new Vector2[3];

        for (int i = 0; i < 3; i++)
        {
            vertices[i] = meshToCut.vertices[triangleVertexIndices[i]];
            normals[i] = meshToCut.normals[triangleVertexIndices[i]];
            uvs[i] = meshToCut.uv[triangleVertexIndices[i]];
            if (meshToCut.uv2.Length > 0)
                rtLightmapUvs[i] = meshToCut.uv2[triangleVertexIndices[i]];
            else
                rtLightmapUvs[i] = new Vector2(0, 0);               //--> Quickfix. Todo: Fix properly later
        }

        return new MeshTriangle(vertices, normals, uvs, rtLightmapUvs, submeshIndex);
    }

    //Function that cuts Triangles in three steps, sorting, calculating new position, UV and normals and then converting the calculated data in Mesh triangles
    void CutTriangle(Plane plane, MeshTriangle meshTriangle, bool[] isVertexPositiveSide, GeneratedMesh positiveSide, GeneratedMesh negativeSide, List<Vector3> addedVertices)
    {
        //placeholder Triangles: max 2 Verteces per Side, so one side gets 2 unique verteces and the other one 2 equal ones
        MeshTriangle positiveSideMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], new Vector2[2], meshTriangle.getSubmeshIndex());
        MeshTriangle negativeSideMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], new Vector2[2], meshTriangle.getSubmeshIndex());

        bool positiveVertexExistent = false;
        bool negativeVertexExistent = false;

        #region sorting
        for (int i = 0; i < 3; i++)
        {
            if (isVertexPositiveSide[i])
            {
                if (!positiveVertexExistent)
                {
                    positiveVertexExistent = true;

                    positiveSideMeshTriangle.getVertices()[0] = meshTriangle.getVertices()[i];
                    positiveSideMeshTriangle.getVertices()[1] = positiveSideMeshTriangle.getVertices()[0];

                    positiveSideMeshTriangle.getNormals()[0] = meshTriangle.getNormals()[i];
                    positiveSideMeshTriangle.getNormals()[1] = positiveSideMeshTriangle.getNormals()[0];

                    positiveSideMeshTriangle.getUvs()[0] = meshTriangle.getUvs()[i];
                    positiveSideMeshTriangle.getUvs()[1] = positiveSideMeshTriangle.getUvs()[0];

                    positiveSideMeshTriangle.getRtLightmapUvs()[0] = meshTriangle.getRtLightmapUvs()[i];
                    positiveSideMeshTriangle.getRtLightmapUvs()[1] = positiveSideMeshTriangle.getRtLightmapUvs()[0];
                }
                else
                {
                    positiveSideMeshTriangle.getVertices()[1] = meshTriangle.getVertices()[i];
                    positiveSideMeshTriangle.getNormals()[1] = meshTriangle.getNormals()[i];
                    positiveSideMeshTriangle.getUvs()[1] = meshTriangle.getUvs()[i];
                    positiveSideMeshTriangle.getRtLightmapUvs()[1] = meshTriangle.getRtLightmapUvs()[i];
                }
            }
            else
            {
                if (!negativeVertexExistent)
                {
                    negativeVertexExistent = true;

                    negativeSideMeshTriangle.getVertices()[0] = meshTriangle.getVertices()[i];
                    negativeSideMeshTriangle.getVertices()[1] = negativeSideMeshTriangle.getVertices()[0];

                    negativeSideMeshTriangle.getNormals()[0] = meshTriangle.getNormals()[i];
                    negativeSideMeshTriangle.getNormals()[1] = negativeSideMeshTriangle.getNormals()[0];

                    negativeSideMeshTriangle.getUvs()[0] = meshTriangle.getUvs()[i];
                    negativeSideMeshTriangle.getUvs()[1] = negativeSideMeshTriangle.getUvs()[0];

                    negativeSideMeshTriangle.getRtLightmapUvs()[0] = meshTriangle.getRtLightmapUvs()[i];
                    negativeSideMeshTriangle.getRtLightmapUvs()[1] = negativeSideMeshTriangle.getRtLightmapUvs()[0];
                }
                else
                {
                    negativeSideMeshTriangle.getVertices()[1] = meshTriangle.getVertices()[i];
                    negativeSideMeshTriangle.getNormals()[1] = meshTriangle.getNormals()[i];
                    negativeSideMeshTriangle.getUvs()[1] = meshTriangle.getUvs()[i];
                    negativeSideMeshTriangle.getRtLightmapUvs()[1] = meshTriangle.getRtLightmapUvs()[i];
                }
            }

        }
        #endregion

        #region calculating position, UV and normals to add
        float distance, normalizedDistance;
        plane.Raycast(new Ray(positiveSideMeshTriangle.getVertices()[0], (negativeSideMeshTriangle.getVertices()[0] - positiveSideMeshTriangle.getVertices()[0]).normalized), out distance);

        normalizedDistance = distance / (negativeSideMeshTriangle.getVertices()[0] - positiveSideMeshTriangle.getVertices()[0]).magnitude;
        Vector3 vertexPositive = Vector3.Lerp(positiveSideMeshTriangle.getVertices()[0], negativeSideMeshTriangle.getVertices()[0], normalizedDistance);
        addedVertices.Add(vertexPositive);

        Vector3 normalPositive = Vector3.Lerp(positiveSideMeshTriangle.getNormals()[0], negativeSideMeshTriangle.getNormals()[0], normalizedDistance);
        Vector2 uvPositive = Vector2.Lerp(positiveSideMeshTriangle.getUvs()[0], negativeSideMeshTriangle.getUvs()[0], normalizedDistance);
        Vector2 rtLightmapUvsPositive = Vector2.Lerp(positiveSideMeshTriangle.getRtLightmapUvs()[0], negativeSideMeshTriangle.getRtLightmapUvs()[0], normalizedDistance);


        plane.Raycast(new Ray(positiveSideMeshTriangle.getVertices()[1], (negativeSideMeshTriangle.getVertices()[1] - positiveSideMeshTriangle.getVertices()[1]).normalized), out distance);

        normalizedDistance = distance / (negativeSideMeshTriangle.getVertices()[1] - positiveSideMeshTriangle.getVertices()[1]).magnitude;
        Vector3 vertexNegative = Vector3.Lerp(positiveSideMeshTriangle.getVertices()[1], negativeSideMeshTriangle.getVertices()[1], normalizedDistance);
        addedVertices.Add(vertexNegative);

        Vector3 normalNegative = Vector3.Lerp(positiveSideMeshTriangle.getNormals()[1], negativeSideMeshTriangle.getNormals()[1], normalizedDistance);
        Vector2 uvNegative = Vector2.Lerp(positiveSideMeshTriangle.getUvs()[1], negativeSideMeshTriangle.getUvs()[1], normalizedDistance);
        Vector2 rtLightmapUvsNegative = Vector2.Lerp(positiveSideMeshTriangle.getRtLightmapUvs()[1], negativeSideMeshTriangle.getRtLightmapUvs()[1], normalizedDistance);
        #endregion

        #region converting data to mesh triangles and adding it to the mesh if its not already part of it
        Vector3[] generatedVertices = new Vector3[] { positiveSideMeshTriangle.getVertices()[0], vertexPositive, vertexNegative };
        Vector3[] generatedNormals = new Vector3[] { positiveSideMeshTriangle.getNormals()[0], normalPositive, normalNegative };
        Vector2[] generatedUvs = new Vector2[] { positiveSideMeshTriangle.getUvs()[0], uvPositive, uvNegative };
        Vector2[] generatedRtLightmapUvs = new Vector2[] { positiveSideMeshTriangle.getRtLightmapUvs()[0], rtLightmapUvsPositive, rtLightmapUvsNegative };

        if (generatedVertices[0] != generatedVertices[1] && generatedVertices[0] != generatedVertices[2])
            AddGeneratedTriangle(positiveSide, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, meshTriangle.getSubmeshIndex());

        generatedVertices = new Vector3[] { positiveSideMeshTriangle.getVertices()[0], positiveSideMeshTriangle.getVertices()[1], vertexNegative };
        generatedNormals = new Vector3[] { positiveSideMeshTriangle.getNormals()[0], positiveSideMeshTriangle.getNormals()[1], normalNegative };
        generatedUvs = new Vector2[] { positiveSideMeshTriangle.getUvs()[0], positiveSideMeshTriangle.getUvs()[1], uvNegative };
        generatedRtLightmapUvs = new Vector2[] { positiveSideMeshTriangle.getRtLightmapUvs()[0], positiveSideMeshTriangle.getRtLightmapUvs()[1], rtLightmapUvsNegative };

        if (generatedVertices[0] != generatedVertices[1] && generatedVertices[0] != generatedVertices[2])
            AddGeneratedTriangle(positiveSide, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, meshTriangle.getSubmeshIndex()); ;

        generatedVertices = new Vector3[] { negativeSideMeshTriangle.getVertices()[0], vertexPositive, vertexNegative };
        generatedNormals = new Vector3[] { negativeSideMeshTriangle.getNormals()[0], normalPositive, normalNegative };
        generatedUvs = new Vector2[] { negativeSideMeshTriangle.getUvs()[0], uvPositive, uvNegative };
        generatedRtLightmapUvs = new Vector2[] { negativeSideMeshTriangle.getRtLightmapUvs()[0], rtLightmapUvsPositive, rtLightmapUvsNegative };

        if (generatedVertices[0] != generatedVertices[1] && generatedVertices[0] != generatedVertices[2])
            AddGeneratedTriangle(negativeSide, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, meshTriangle.getSubmeshIndex());

        generatedVertices = new Vector3[] { negativeSideMeshTriangle.getVertices()[0], negativeSideMeshTriangle.getVertices()[1], vertexNegative };
        generatedNormals = new Vector3[] { negativeSideMeshTriangle.getNormals()[0], negativeSideMeshTriangle.getNormals()[1], normalNegative };
        generatedUvs = new Vector2[] { negativeSideMeshTriangle.getUvs()[0], negativeSideMeshTriangle.getUvs()[1], uvNegative };
        generatedRtLightmapUvs = new Vector2[] { negativeSideMeshTriangle.getRtLightmapUvs()[0], negativeSideMeshTriangle.getRtLightmapUvs()[1], rtLightmapUvsNegative };

        if (generatedVertices[0] != generatedVertices[1] && generatedVertices[0] != generatedVertices[2])
            AddGeneratedTriangle(negativeSide, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, meshTriangle.getSubmeshIndex());
        #endregion
    }

    //Function that adds a triangle to the parsed in meshToAdd, while flipping it if it looks the wrong way
    void AddGeneratedTriangle(GeneratedMesh meshToAdd, Vector3[] generatedVertices, Vector3[] generatedNormals, Vector2[] generatedUvs, Vector2[] generatedRtLightmapUvs, int submeshIndex)
    {
        MeshTriangle generatedTriangle = new MeshTriangle(generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, submeshIndex);

        //Cross product = normal of two vertices, dot product
        //v * w > 0 == Vectors look at same side of plane || v * w < 0 == Vectors look at different of plane
        if (Vector3.Dot(Vector3.Cross(generatedVertices[1] - generatedVertices[0], generatedVertices[2] - generatedVertices[0]), generatedNormals[0]) < 0)
        {
            FlipTriangle(generatedTriangle);
        }

        meshToAdd.addTriangle(generatedTriangle);
    }

    //Function switches the position of the first and last vertice, normal and uv with each other
    void FlipTriangle(MeshTriangle meshTriangle)
    {
        List<Vector3> triangleVerticies = meshTriangle.getVertices();
        Vector3 lastVertex = triangleVerticies[triangleVerticies.Count - 1];
        triangleVerticies[triangleVerticies.Count - 1] = triangleVerticies[0];
        triangleVerticies[0] = lastVertex;


        List<Vector3> triangleNormals = meshTriangle.getNormals();
        Vector3 lastNormal = triangleNormals[triangleNormals.Count - 1];
        triangleNormals[triangleNormals.Count - 1] = triangleNormals[0];
        triangleNormals[0] = lastNormal;


        List<Vector2> triangleUvs = meshTriangle.getUvs();
        Vector2 lastUV = triangleUvs[triangleUvs.Count - 1];
        triangleUvs[triangleUvs.Count - 1] = triangleUvs[0];
        triangleUvs[0] = lastUV;

        List<Vector2> triangleRtLightmapUvs = meshTriangle.getRtLightmapUvs();
        Vector2 lastRtLightmapUvs = triangleRtLightmapUvs[triangleRtLightmapUvs.Count - 1];
        triangleRtLightmapUvs[triangleRtLightmapUvs.Count - 1] = triangleRtLightmapUvs[0];
        triangleRtLightmapUvs[0] = lastRtLightmapUvs;
    }
    #endregion

    #region Capping
    //Function that creates polygones out of all cut vertices using a helper function and then parses these polygons into the actual fill function
    void FillCut(List<Vector3> addedVertices, Plane plane, GeneratedMesh positiveMesh, GeneratedMesh negativeMesh, string cutMaterialName, string[] rendererMaterialNames)
    {
        List<Vector3> processedVertices = new List<Vector3>(), polygone = new List<Vector3>();

        for (int i = 0; i < addedVertices.Count - 1; i++)
        {           //-1 Custom Edit, trying to prevent OFB excepton
            if (!processedVertices.Contains(addedVertices[i]))
            {
                polygone = new List<Vector3>();
                polygone.Add(addedVertices[i]);
                polygone.Add(addedVertices[i + 1]);          //OUT OF BOUNDS EXCEPTION here sometimes

                processedVertices.Add(addedVertices[i]);
                processedVertices.Add(addedVertices[i + 1]);

                FindConnectedVertices(addedVertices, processedVertices, polygone);
                Fill(polygone, plane, positiveMesh, negativeMesh, cutMaterialName, rendererMaterialNames);
            }
        }
    }

    //Function that connects parsed in vertices by finding their middle point (like with an Orange). It then addes those newly created vertices to both sides of the cut Mesh
    void Fill(List<Vector3> vertices, Plane plane, GeneratedMesh positiveMesh, GeneratedMesh negativeMesh, string cutMaterialName, string[] rendererMaterialNames)
    {
        //finding the center
        Vector3 center = new Vector3();
        for (int i = 0; i < vertices.Count; i++)
        {
            center += vertices[i];
        }
        center /= vertices.Count;

        Vector3 up = plane.normal;
        Vector3 left = new Vector3(-up.y, up.x, 0);
        Vector3 forward = Quaternion.AngleAxis(90, up) * left;

        Vector3 displacement = new Vector3();
        Vector2 uv1, uv2;
        Vector2 rtLightmapUvs1, rtLightmapUvs2;

        for (int i = 0; i < vertices.Count; i++)
        {
            displacement = vertices[i] - center;
            uv1 = new Vector2(.5f + Vector3.Dot(displacement, left), .5f + Vector3.Dot(displacement, forward));
            rtLightmapUvs1 = new Vector2(.5f + Vector3.Dot(displacement, left), .5f + Vector3.Dot(displacement, forward));   //this generated a unprecise position on the Meshs surface. TODO

            displacement = vertices[(i + 1) % vertices.Count] - center;
            uv2 = new Vector2(.5f + Vector3.Dot(displacement, left), .5f + Vector3.Dot(displacement, forward));
            rtLightmapUvs2 = new Vector2(.5f + Vector3.Dot(displacement, left), .5f + Vector3.Dot(displacement, forward));

            Vector3[] generatedVertices = new Vector3[] { vertices[i], vertices[(i + 1) % vertices.Count], center };
            Vector3[] generatedNormals = new Vector3[] { -up, -up, -up };
            Vector2[] generatedUvs = new Vector2[] { uv1, uv2, new Vector2(0.5f, 0.5f) };
            Vector2[] generatedRtLightmapUvs = new Vector2[] { rtLightmapUvs1, rtLightmapUvs2, new Vector2(0.5f, 0.5f) };

            //don't create different submesh layer if cutMaterial isn't set
            int submeshIndex = 0;

            //create it if it isn't
            if (cutMaterial != null)
            {
                if (!DoesIncludeMaterialString(rendererMaterialNames, cutMaterialName))
                {
                    submeshIndex = rendererMaterialNames.Length;
                }
                else
                {
                    for (int j = 0; j < renderer.materials.Length; j++)
                    {
                        if (CompareMaterialNamesString(rendererMaterialNames[j], cutMaterialName))
                            submeshIndex = j;
                    }
                }
            }

            AddGeneratedTriangle(positiveMesh, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, submeshIndex);

            generatedNormals = new Vector3[] { up, up, up };
            AddGeneratedTriangle(negativeMesh, generatedVertices, generatedNormals, generatedUvs, generatedRtLightmapUvs, submeshIndex);
        }
    }

    //Function that creates polygones out of all cut vertices
    void FindConnectedVertices(List<Vector3> addedVertices, List<Vector3> processedVertices, List<Vector3> polygone)
    {
        bool isDone = false;
        while (!isDone)
        {
            isDone = true;
            for (int i = 0; i < addedVertices.Count; i += 2)
            {
                if (addedVertices[i] == polygone[polygone.Count - 1] && !processedVertices.Contains(addedVertices[i + 1]))
                {
                    isDone = false;
                    polygone.Add(addedVertices[i + 1]);
                    processedVertices.Add(addedVertices[i + 1]);
                }
                else if (addedVertices[i + 1] == polygone[polygone.Count - 1] && !processedVertices.Contains(addedVertices[i]))
                {
                    isDone = false;
                    polygone.Add(addedVertices[i]);
                    processedVertices.Add(addedVertices[i]);
                }
            }
        }
    }
    #endregion

    #region Mesh creation
    //Function that creates interactable gameObjects within the scene using the root Objects Data and the generated mesh data
    GameObject CreateGameObject(GeneratedMesh newMeshData)
    {
        Debug.Log("Create game object called");
        //if the colliders of a Mesh are bigger than the Mesh itself, so the Function was triggered but no triangle was cut
        if (newMeshData.getVertices().Count > 0)
        {
            //creating copy of the game Object that is getting cut
            GameObject cutGameObject = GameObject.Instantiate(gameObjectToCut, position, rotation);

            if (cutGameObject.GetComponent<MoveGOPivot>()) cutGameObject.GetComponent<MoveGOPivot>().pivotWasCentered = false;
            cutGameObject.transform.SetParent(gameObjectToCut.transform.parent);

            cutGameObject.transform.localPosition = localPosition;
            cutGameObject.transform.localRotation = gameObjectToCut.transform.localRotation;

            if (negativelyScaledSkinnedMesh) cutGameObject.transform.localScale = new Vector3(-cutGameObject.transform.localScale.x, cutGameObject.transform.localScale.y, cutGameObject.transform.localScale.z);

            Mesh generatedMesh = cutGameObject.GetComponent<MeshFilter>().mesh;
            generatedMesh.Clear();

            //set new materials Array when a cut Material is to be added
            if (cutMaterial != null && cap)
            {
                Material[] materials;
                //adds cut Material to submesh layers if it isn't already part of it while copying all Materials that are part of the Mesh
                if (!DoesIncludeMaterial(gameObjectToCut.GetComponent<Renderer>().materials, cutMaterial))
                {
                    generatedMesh.subMeshCount = meshToCut.subMeshCount + 1;

                    materials = new Material[cutGameObject.GetComponent<Renderer>().materials.Length + 1];
                    materials[materials.Length - 1] = cutMaterial;
                }
                //copies all Materials that are part of the Mesh
                else
                {
                    generatedMesh.subMeshCount = meshToCut.subMeshCount;

                    materials = new Material[cutGameObject.GetComponent<Renderer>().materials.Length];
                }

                for (int i = 0; i < cutGameObject.GetComponent<Renderer>().materials.Length; i++)
                {
                    materials[i] = cutGameObject.GetComponent<Renderer>().materials[i];
                }

                cutGameObject.GetComponent<Renderer>().materials = materials;
            }

            //writes all mesh Data into the mesh
            generatedMesh.vertices = newMeshData.getVertices().ToArray();
            generatedMesh.normals = newMeshData.getNormals().ToArray();
            generatedMesh.SetUVs(0, newMeshData.getUvs());
            generatedMesh.SetUVs(1, newMeshData.getRtLightmapUvs());

            for (int i = 0; i < newMeshData.getSubmeshIndices().Count; i++)
            {
                generatedMesh.SetTriangles(newMeshData.getSubmeshIndices()[i].ToArray(), i);
            }

            //generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateTangents();    //this could potentially be slow. Maybe copy all tangens by hand    --> GENERATES TANGENS FOR NORMAL MAPS

            foreach (Collider col in cutGameObject.GetComponents<Collider>())
                GameObject.Destroy(col);

            //creates mesh Collider for Object
            MeshCollider meshCollider = cutGameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = cutGameObject.GetComponent<MeshFilter>().mesh;

            //this is for Hand cutting logic
            return cutGameObject;
        }
        return null;
    }

    GameObject CreateGameObjectAfterSeperatingMesh(GeneratedMesh newMeshData, GameObject goToCopy)
    {
        //if the colliders of a Mesh are bigger than the Mesh itself, so the Function was triggered but no triangle was cut
        if (newMeshData.getVertices().Count > 0)
        {
            //creating copy of the game Object that is getting cut
            GameObject cutGameObject = GameObject.Instantiate(goToCopy);

            cutGameObject.transform.localPosition = goToCopy.transform.localPosition;
            cutGameObject.transform.localRotation = goToCopy.transform.localRotation;

            if (negativelyScaledSkinnedMesh) cutGameObject.transform.localScale = new Vector3(-cutGameObject.transform.localScale.x, cutGameObject.transform.localScale.y, cutGameObject.transform.localScale.z);

            Mesh generatedMesh = cutGameObject.GetComponent<MeshFilter>().mesh;
            generatedMesh.Clear();

            //set new materials Array when a cut Material is to be added
            if (cutMaterial != null && cap)
            {
                Material[] materials;
                //adds cut Material to submesh layers if it isn't already part of it while copying all Materials that are part of the Mesh
                if (!DoesIncludeMaterial(goToCopy.GetComponent<Renderer>().materials, cutMaterial))
                {
                    generatedMesh.subMeshCount = goToCopy.GetComponent<MeshFilter>().mesh.subMeshCount + 1;

                    materials = new Material[cutGameObject.GetComponent<Renderer>().materials.Length + 1];
                    materials[materials.Length - 1] = cutMaterial;
                }
                //copies all Materials that are part of the Mesh
                else
                {
                    generatedMesh.subMeshCount = goToCopy.GetComponent<MeshFilter>().mesh.subMeshCount;

                    materials = new Material[cutGameObject.GetComponent<Renderer>().materials.Length];
                }

                for (int i = 0; i < cutGameObject.GetComponent<Renderer>().materials.Length; i++)
                {
                    materials[i] = cutGameObject.GetComponent<Renderer>().materials[i];
                }

                cutGameObject.GetComponent<Renderer>().materials = materials;
            }

            //writes all mesh Data into the mesh
            generatedMesh.vertices = newMeshData.getVertices().ToArray();
            generatedMesh.normals = newMeshData.getNormals().ToArray();
            generatedMesh.SetUVs(0, newMeshData.getUvs());
            generatedMesh.SetUVs(1, newMeshData.getRtLightmapUvs());

            for (int i = 0; i < newMeshData.getSubmeshIndices().Count; i++)
            {
                generatedMesh.SetTriangles(newMeshData.getSubmeshIndices()[i].ToArray(), i);
            }

            //generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateTangents();    //this could potentially be slow. Maybe copy all tangens by hand    --> GENERATES TANGENS FOR NORMAL MAPS

            foreach (Collider col in cutGameObject.GetComponents<Collider>())
                GameObject.Destroy(col);

            //creates mesh Collider for Object
            MeshCollider meshCollider = cutGameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = cutGameObject.GetComponent<MeshFilter>().mesh;

            //this is for Hand cutting logic
            return cutGameObject;
        }
        return null;
    }

    //Function that finds seperated Meshes within one GeneratedMesh and initializes them all seperately
    void FindSeperatedMeshes(GeneratedMesh meshToSeperate)
    {
        List<int> connectedVertexIndeces, leftoverVertexIndeces = new List<int>();

        int numberOfVertexIndeces = 0;

        foreach (List<int> vertexIndeces in meshToSeperate.getSubmeshIndices())
        {
            leftoverVertexIndeces.AddRange(vertexIndeces);
            numberOfVertexIndeces += vertexIndeces.Count;
        }

        while (leftoverVertexIndeces.Count > 0)
        {
            connectedVertexIndeces = new List<int>();
            int[] currentTriangle = new int[] { leftoverVertexIndeces[0], leftoverVertexIndeces[1], leftoverVertexIndeces[2] };

            leftoverVertexIndeces.RemoveRange(0, 3);

            //holds all Vertex Indeces that are connected with one another
            connectedVertexIndeces = GetConnectedTriangles(currentTriangle, leftoverVertexIndeces, meshToSeperate);

            //if there's no seperation
            if (connectedVertexIndeces.Count == numberOfVertexIndeces)
            {
                CreateGameObject(meshToSeperate);
                break;
            }
            else
            {
                GeneratedMesh seperatedMesh = new GeneratedMesh();

                for (int i = 0; i < connectedVertexIndeces.Count; i += 3)
                {
                    //uses data in connectedVertexIndeces to copy all Mesh data into a new GeneratedMesh
                    Vector3[] vertices = new Vector3[3] {meshToSeperate.getVertices()[connectedVertexIndeces[i]],
                        meshToSeperate.getVertices()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getVertices()[connectedVertexIndeces[i+2]]};
                    Vector3[] normals = new Vector3[3] {meshToSeperate.getNormals()[connectedVertexIndeces[i]],
                        meshToSeperate.getNormals()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getNormals()[connectedVertexIndeces[i+2]]};
                    Vector2[] Uvs = new Vector2[3] {meshToSeperate.getUvs()[connectedVertexIndeces[i]],
                        meshToSeperate.getUvs()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getUvs()[connectedVertexIndeces[i+2]]};
                    Vector2[] rtLightmapUvs = new Vector2[3] {meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i]],
                        meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i+2]]};

                    int submeshIndex = 0;
                    if (meshToSeperate.getSubmeshIndices().Count > 1)
                    {
                        //Parses all submeshes of mesh until it finds the one holding our verteces
                        for (int j = 0; j < meshToSeperate.getSubmeshIndices().Count; j++)
                        {
                            if (meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i]) &&
                                meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i + 1]) &&
                                meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i + 2]))
                            {
                                submeshIndex = j;
                                break;
                            }
                        }
                    }

                    MeshTriangle meshTriangle = new MeshTriangle(vertices, normals, Uvs, rtLightmapUvs, submeshIndex);

                    seperatedMesh.addTriangle(meshTriangle);

                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i]);
                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i + 1]);
                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i + 2]);
                }

                CreateGameObject(seperatedMesh);
            }
        }
    }

    async void StartAsyncMeshSeperation(GameObject goToSeperate)
    {
        GeneratedMesh meshToSeperate = new GeneratedMesh();

        #region duplicate goToSeperate as GeneratedMesh
        List<int[]> goToCutVertexIndices = new List<int[]>();

        for (int i = 0; i < goToSeperate.GetComponent<MeshFilter>().mesh.subMeshCount; i++)
        {
            goToCutVertexIndices.Add(goToSeperate.GetComponent<MeshFilter>().mesh.GetTriangles(i));
        }

        Vector3[] meshVertices = goToSeperate.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] meshNormals = goToSeperate.GetComponent<MeshFilter>().mesh.normals;
        Vector2[] meshUvs = goToSeperate.GetComponent<MeshFilter>().mesh.uv;
        Vector2[] meshRtLightmapUvs = goToSeperate.GetComponent<MeshFilter>().mesh.uv2; //this are the global illumination map UVs

        //get current Triangle
        Vector3[] vertices = new Vector3[3];
        Vector3[] normals = new Vector3[3];
        Vector2[] uvs = new Vector2[3];
        Vector2[] rtLightmapUvs = new Vector2[3];

        for (int i = 0; i < goToCutVertexIndices.Count; i++)
        {
            //going through each Triangle within the submesh
            for (int j = 0; j < goToCutVertexIndices[i].Length; j += 3)
            {
                for (int k = 0; k < 3; k++)
                {
                    vertices[k] = meshVertices[goToCutVertexIndices[i][j + k]];
                    normals[k] = meshNormals[goToCutVertexIndices[i][j + k]];
                    uvs[k] = meshUvs[goToCutVertexIndices[i][j + k]];
                    if (meshRtLightmapUvs.Length > 0)
                        rtLightmapUvs[k] = meshRtLightmapUvs[goToCutVertexIndices[i][j + k]];
                    else
                        rtLightmapUvs[k] = new Vector2(0, 0);               //--> Quickfix. Todo: Fix properly later
                }

                MeshTriangle currentTriangle = new MeshTriangle(vertices, normals, uvs, rtLightmapUvs, i);
                meshToSeperate.addTriangle(currentTriangle);
            }

        }
        #endregion

        //Find seperated meshs async
        List<GeneratedMesh> generatedMeshs = await Task.Run(() => FindSeperatedMeshesWithReturn(meshToSeperate));

        //create object as soon as cutting is done
        foreach (GeneratedMesh generatedMesh in generatedMeshs)
        {
            GameObject go = CreateGameObjectAfterSeperatingMesh(generatedMesh, goToSeperate);

            go.transform.parent = null;
            go.GetComponent<Rigidbody>().useGravity = true;

            //this is the players hand
            SpawnBlood testBloodInstantiation = go.GetComponent<SpawnBlood>();
            testBloodInstantiation.InvokeBleedFromCutterClass(go.transform.position - go.GetComponent<Renderer>().bounds.center);
        }

        GameObject.Destroy(goToSeperate);
    }


    //Function that finds seperated Meshes within one GeneratedMesh and initializes them all seperately
    List<GeneratedMesh> FindSeperatedMeshesWithReturn(GeneratedMesh meshToSeperate)
    {
        List <GeneratedMesh> seperatedMeshs = new List<GeneratedMesh>();

        List<int> connectedVertexIndeces, leftoverVertexIndeces = new List<int>();

        int numberOfVertexIndeces = 0;

        foreach (List<int> vertexIndeces in meshToSeperate.getSubmeshIndices())
        {
            leftoverVertexIndeces.AddRange(vertexIndeces);
            numberOfVertexIndeces += vertexIndeces.Count;
        }

        while (leftoverVertexIndeces.Count > 0)
        {
            connectedVertexIndeces = new List<int>();
            int[] currentTriangle = new int[] { leftoverVertexIndeces[0], leftoverVertexIndeces[1], leftoverVertexIndeces[2] };

            leftoverVertexIndeces.RemoveRange(0, 3);

            //holds all Vertex Indeces that are connected with one another
            connectedVertexIndeces = GetConnectedTriangles(currentTriangle, leftoverVertexIndeces, meshToSeperate);

            //if there's no seperation
            if (connectedVertexIndeces.Count == numberOfVertexIndeces)
            {
                seperatedMeshs.Add(meshToSeperate);
                return seperatedMeshs;
            }
            else
            {
                GeneratedMesh seperatedMesh = new GeneratedMesh();

                for (int i = 0; i < connectedVertexIndeces.Count; i += 3)
                {
                    //uses data in connectedVertexIndeces to copy all Mesh data into a new GeneratedMesh
                    Vector3[] vertices = new Vector3[3] {meshToSeperate.getVertices()[connectedVertexIndeces[i]],
                        meshToSeperate.getVertices()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getVertices()[connectedVertexIndeces[i+2]]};
                    Vector3[] normals = new Vector3[3] {meshToSeperate.getNormals()[connectedVertexIndeces[i]],
                        meshToSeperate.getNormals()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getNormals()[connectedVertexIndeces[i+2]]};
                    Vector2[] Uvs = new Vector2[3] {meshToSeperate.getUvs()[connectedVertexIndeces[i]],
                        meshToSeperate.getUvs()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getUvs()[connectedVertexIndeces[i+2]]};
                    Vector2[] rtLightmapUvs = new Vector2[3] {meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i]],
                        meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i+1]],
                        meshToSeperate.getRtLightmapUvs()[connectedVertexIndeces[i+2]]};

                    int submeshIndex = 0;
                    if (meshToSeperate.getSubmeshIndices().Count > 1)
                    {
                        //Parses all submeshes of mesh until it finds the one holding our verteces
                        for (int j = 0; j < meshToSeperate.getSubmeshIndices().Count; j++)
                        {
                            if (meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i]) &&
                                meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i + 1]) &&
                                meshToSeperate.getSubmeshIndices()[j].Contains(connectedVertexIndeces[i + 2]))
                            {
                                submeshIndex = j;
                                break;
                            }
                        }
                    }

                    MeshTriangle meshTriangle = new MeshTriangle(vertices, normals, Uvs, rtLightmapUvs, submeshIndex);

                    seperatedMesh.addTriangle(meshTriangle);

                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i]);
                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i + 1]);
                    leftoverVertexIndeces.Remove(connectedVertexIndeces[i + 2]);
                }

                seperatedMeshs.Add(seperatedMesh);
            }
        }

        return seperatedMeshs;
    }

    //Function that finds all connected vertex Indeces recursively for a picked Triangle
    public List<int> GetConnectedTriangles(int[] pickedTriangle, List<int> vertexIndeces, GeneratedMesh meshToSeperate)
    {
        List<int> result = new List<int>(pickedTriangle), neighbors = new List<int>(), indecesOfIndecesToRemove = new List<int>();

        // Finding all neigbor Triangles
        for (int i = 0; i < vertexIndeces.Count; i += 3)
        {
            int[] currentTriangle = new int[3] { vertexIndeces[i], vertexIndeces[i + 1], vertexIndeces[i + 2] };

            // Check if Triangles share common Edge
            if (IsConnected(ConvertIndecesToVerteces(currentTriangle, meshToSeperate), ConvertIndecesToVerteces(pickedTriangle, meshToSeperate)))
            {
                neighbors.AddRange(currentTriangle);

                indecesOfIndecesToRemove.Add(i);
                indecesOfIndecesToRemove.Add(i + 1);
                indecesOfIndecesToRemove.Add(i + 2);

                //performance boost: cancel this operation when neighbors length is 3 in first call and 2 in every single other call
            }
        }

        //remove all already connected Trinagles out of the vertexIndeces List
        for (int i = indecesOfIndecesToRemove.Count - 1; i >= 0; i--)
        {
            vertexIndeces.RemoveAt(indecesOfIndecesToRemove[i]);
        }

        //recall this function recursively to find all existing neighbors
        for (int i = 0; i < neighbors.Count; i += 3)
        {
            int[] currentTriangle = new int[3] { neighbors[i], neighbors[i + 1], neighbors[i + 2] };

            // Recursively add all the linked faces to the result
            result.AddRange(GetConnectedTriangles(currentTriangle, vertexIndeces, meshToSeperate));
        }

        return result;
    }

    //Function that checks if the Edges of two Verteces are similar to one another
    bool IsConnected(Vector3[] triangleA, Vector3[] triangleB)
    {
        for (int i = 0; i < triangleA.Length; i++)
            for (int j = 0; j < triangleB.Length; j++)
                if (triangleA[i] == triangleB[j] &&
                    ((triangleA[(i + 1) % 3] == triangleB[(j + 1) % 3] || triangleA[(i + 1) % 3] == triangleB[(j + 2) % 3]) ||
                    (triangleA[(i + 2) % 3] == triangleB[(j + 1) % 3] || triangleA[(i + 2) % 3] == triangleB[(j + 2) % 3])))
                    return true;

        return false;
    }

    //Function that converts vertex Indeces into their respective Vertex
    Vector3[] ConvertIndecesToVerteces(int[] vertexIndices, GeneratedMesh meshData)
    {
        Vector3[] vecs = new Vector3[vertexIndices.Length];

        for (int i = 0; i < vecs.Length; i++)
        {
            vecs[i] = meshData.getVertices()[vertexIndices[i]];
        }

        return vecs;
    }

    //returns if list of material Instances does include root material
    bool DoesIncludeMaterial(Material[] materials, Material material)
    {
        foreach (Material mat in materials)
        {
            if (CompareMaterialNames(mat, material))
                return true;
        }

        return false;
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

    bool DoesIncludeMaterialString(string[] materials, string materialName)
    {
        foreach (string mat in materials)
        {
            if (CompareMaterialNamesString(mat, materialName))
                return true;
        }

        return false;
    }

    //checking if names are similar and then again by removing excess name on both sides: E.g. (Instance) tag
    bool CompareMaterialNamesString(string materialAName, string materialBName)
    {
        if (materialAName == materialBName)
            return true;

        if (materialAName.Length > materialBName.Length && materialAName.Substring(0, materialBName.Length) == materialBName)
            return true;

        if (materialAName.Length < materialBName.Length && materialBName.Substring(0, materialAName.Length) == materialAName)
            return true;

        return false;
    }
    #endregion
}
