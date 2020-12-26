using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


[ExecuteAlways]
public class Tree : MonoBehaviour
{



    [Header("Branching Info")]
    // How many points we see on a branch
    
    [Range(1,100)] public int pointsPerBranch = 40;
    
    [Range(0,2)] public float pointsPerBranchReducer = .9f;

    // Max number of spawns per branch
    [Range(1,100)] public int maxNumberBranchesPerBranch = 40;
    
    [Range(0,2)] public float maxNumberBranchesPerBranchReducer = .6f;


    [Range(0,1)]public float branchChance = .6f;
    
    [Range(0,2)] public float branchChanceReducer = .8f;


    [Range(0,1)]public float noisePower = .4f;
    [Range(0,2)] public float noisePowerReducer = 1.1f;


    [Range(0,10)]public float noiseSize = .4f;
    [Range(0,5)] public float noiseSizeReducer = 1.1f;
    
    
    // Changes how much branches match the current branches
    // direction
    [Range(0,1)]public float minAngleMatch = 0;
    
    [Range(0,2)] public float minAngleReducer = 1;
    
    [Range(0,1)] public float maxAngleMatch = 1;
    
    [Range(0,2)] public float maxAngleReducer = .9f;





    // If this value is 1, then the length will be reduced
    // by how far up the branch it is. if it is 0, will not matter
    [Range(0,1)] public float baseVsTipLength = 0;
    [Range(0,2)] public float baseVsTipLengthReducer = 1;


    // If this value is 1, then the branch will pull its max length from 
    // its parents length, rather than the current iteration level
    [Range(0,1)] public float parentLengthMax= 0;
    [Range(0,2)] public float parentLengthMaxReducer = 1;



    [Range(0,5)] public float length = 2;
    [Range(0,2)] public float lengthReducer = .9f;


    [Range(0,1)] public float lengthVariation = .3f;
    [Range(0,2)] public float lengthVariationReducer = .9f;

    [Range(0,1)] public float upDesire = 1;    
    [Range(0,2)] public float upDesireReducer = .9f;

    [Header("Bark Info")]
    [Range(0,.5f)] public float width;
    [Range(0,1)] public float widthReducer;


    // if width smaller than this wont make a branch
    [Range(0,.1f)] public float widthCutoff;

    [Range(0,100)] public int numBarkColumns = 10;
    [Range(0,2)] public float numBarkColumnsReducer = 1;

    [Range(0,100)] public int numBarkRows = 20;
    
    [Range(0,2)] public float numBarkRowsReducer = 1;


    
    // Width of just the trunk
    public AnimationCurve trunkCurve;

    // width of branches
    public AnimationCurve branchCurve;
    





    [Header("Limits ")]
    // Limiting recursion
    [Range(0,5)] public int maxIterationLevel = 3;
    [Range(0,10000)] public int maxBranches = 20;
    [Range(0,100000)] public int maxPoints = 100000;




    [Header("Data")]
    public int totalPoints;
    public int totalBarkPoints;
    public int totalBarkTris;
    public int totalFlowerPoints;
    public float maxTime;

    

    public int currentTotalPoints;
    public int currentTotalBranches;

    public List<Branch> branches;//<Branches>

  


    [Header("Rendering")]


    public bool debugSkeleton;
    public ComputeBuffer skeletonBuffer;
    public Material skeletonMaterial;


    public bool debugBark;
    public Material barkMaterial;
    
    public bool debugMesh;
    public Material meshMaterial;
    public ComputeBuffer barkBuffer;
    public ComputeBuffer barkTriBuffer;


    public bool debugFlower;
    public Material flowerMaterial;
    public ComputeBuffer flowerBuffer;

    
    public MaterialPropertyBlock skeletonMPB;
    public MaterialPropertyBlock barkMPB;
    public MaterialPropertyBlock flowerMPB;
    public MaterialPropertyBlock meshMPB;



    [Range(0,1)]
    public float barkShown;

    
    [Range(0,1)]
    public float flowersShown;

      [Range(0,1)]
    public float flowersFallen;
    
    [Range(0,1)]
    public float skeletonShown;



    /*

        Todo Variables


    */

    [Header("Todo")]
    //public FastNoise noise;
    
    [Range(0,100)] public int numFlowers = 40;
    [Range(0,1)] public float flowerSize = .1f;
    [Range(0,1)] public float offsetSize = .1f;

        

    // Where we want branches to start and end
    [Range(0,1)] public float startBranchLocation = .5f;
    [Range(0,2)] public float startBranchReducer = .9f;
    [Range(0,1)] public float endBranchLocation = 1;
    [Range(0,1)] public float endOfBranchWeight = .5f;
    [Range(0,2)] public float endOfBranchWeightReducer = .9f;


    public Mesh mesh;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;


    public Vector3 noiseOffset; // every time we recreate teh tree, changing this offset
                                // will make it so that we get a different looking tree very time


    public void OnEnable(){
        

        BuildBranches();

    } 
    
    public void BuildBranches(){

        print("REBUILDING");
        
        noiseOffset = new Vector3( 0 , Mathf.Sin(Time.time * 10000) * 1000 ,0 );
        branches = new List<Branch>();
    
        int currIterations = 0;
        currentTotalPoints = 0;
        currentTotalBranches = 0;
       
        Vector3 direction = new Vector3(0,1,0);
        Vector3 startPosition = new Vector3(0,0,0);

        Branch trunk = new Branch(  this, currIterations , null  , startPosition , direction , 0 , 0  , length , width );

        branches.Add( trunk );

        BuildMesh();

    }



    // builds the actual mesh from all our data!
    public void BuildMesh(){        


        maxTime = 0;
        totalPoints = 0;


        totalBarkPoints = 0;
        totalBarkTris = 0;
        totalFlowerPoints = 0;
        foreach( Branch b in branches ){
            
            totalBarkPoints += b.numBarkRows * b.numBarkColumns;

            totalBarkTris +=  (b.numBarkRows-1) * (b.numBarkColumns-1) * 3 * 2;

            totalFlowerPoints += b.numFlowers;
        
            for( int i = 0; i < b.points.Count; i++ ){

                maxTime = Mathf.Max(maxTime , b.points[i].timeCreated);

                totalPoints ++;
            }
        
        
        }

    
        float[] barkVals = new float[ totalBarkPoints * 16 ];
        float[] vals = new float[ totalPoints  * 16 ];
        float[] flowerVals = new float[ totalFlowerPoints*4* 16 ];

        int[] barkTris = new int[totalBarkTris]; 
        int[] flowerTris = new int[totalFlowerPoints*3*2]; 

    
        Vector3[] barkPositions = new Vector3[ totalBarkPoints  ];
        Vector2[] barkUVs = new Vector2[ totalBarkPoints  ];
        Vector3[] barkNormals = new Vector3[ totalBarkPoints  ];
        Vector4[] barkTangents = new Vector4[ totalBarkPoints  ];
        Color[] barkColors = new Color[ totalBarkPoints  ];

        int id = 0;
        int branchID = 0;
        int baseBarkID =0;
        int baseFlowerID =0;
        int baseTri = 0;

        foreach( Branch b in branches ){

            // Gets our base point from the flattened
            // point array
            int baseVal = id;

            // tells us how many points are in our current
            // branch!
            int totalPoints = b.numPoints;


            for( int i = 0; i < b.numBarkRows-1; i++){
                for( int j = 0; j < b.numBarkColumns-1; j++){

                    barkTris[ baseTri * 6 + 0 ] = baseBarkID +     i * b.numBarkColumns + j + 0;
                    barkTris[ baseTri * 6 + 1 ] = baseBarkID +     i * b.numBarkColumns + j + 1;
                    barkTris[ baseTri * 6 + 2 ] = baseBarkID + (i+1) * b.numBarkColumns + j + 1;
                    barkTris[ baseTri * 6 + 3 ] = baseBarkID +     i * b.numBarkColumns + j + 0;
                    barkTris[ baseTri * 6 + 4 ] = baseBarkID + (i+1) * b.numBarkColumns + j + 1;
                    barkTris[ baseTri * 6 + 5 ] = baseBarkID + (i+1) * b.numBarkColumns + j + 0;

                    baseTri ++;

                }
            }

            for( int i = 0; i < b.numBarkRows; i++ ){
                for( int j = 0; j < b.numBarkColumns; j++ ){

                    float normalizedRowID = (float)i/((float)b.numBarkRows-1);
                    float normalizedColID = (float)j/((float)b.numBarkColumns-1);

                    Vector3 fPos;
                    Vector3 fDir;
                    Vector3 fTang;
                    Vector3 fNor;
                    Vector3 fCenter;
                    float fLife;
                    float fWidth;

                    b.GetBarkData( normalizedRowID , normalizedColID, out fPos , out fCenter , out fDir , out fNor , out fTang,  out fLife,out fWidth);

                  //  if( i == b.numBarkRows-1 && j == b.numBarkColumns-1){ print("hmm: "+fLife);}



                    barkPositions[ baseBarkID ] = fPos;
                    barkNormals[ baseBarkID ] = fNor;
                    barkTangents[ baseBarkID ] = new Vector4(fTang.x,fTang.y,fTang.z,1);
                    barkUVs[ baseBarkID ] = new Vector2( normalizedColID , normalizedRowID);
                    barkColors[ baseBarkID ] = new Color( fLife/maxTime , fWidth , baseVal , totalPoints );

                    // TODO MAKE THIS 
                    barkVals[ baseBarkID * 16 + 0 ] = fPos.x;
                    barkVals[ baseBarkID * 16 + 1 ] = fPos.y;
                    barkVals[ baseBarkID * 16 + 2 ] = fPos.z;

                    barkVals[ baseBarkID * 16 + 3 ] = fNor.x;
                    barkVals[ baseBarkID * 16 + 4 ] = fNor.y;
                    barkVals[ baseBarkID * 16 + 5 ] = fNor.z;

                    barkVals[ baseBarkID * 16 + 6 ] = fCenter.x;
                    barkVals[ baseBarkID * 16 + 7 ] = fCenter.y;
                    barkVals[ baseBarkID * 16 + 8 ] = fCenter.z;
                    
                    barkVals[ baseBarkID * 16 + 9 ] = normalizedColID;
                    barkVals[ baseBarkID * 16 + 10 ] = normalizedRowID;
                    
                    barkVals[ baseBarkID * 16 + 11 ] = baseVal;
                    barkVals[ baseBarkID * 16 + 12 ] = totalPoints;
                    barkVals[ baseBarkID * 16 + 13 ] = fLife/maxTime;
                    
                    barkVals[ baseBarkID * 16 + 14 ] = 0;
                    barkVals[ baseBarkID * 16 + 15 ] = 0;
                 
                    baseBarkID ++;
                }


            }



            foreach( FlowerPoint p in b.flowers ){

                Vector3 fPos;
                Vector3 fDir;
                Vector3 fTang;
                Vector3 fNor = Vector3.left;
                Vector3 fCenter;
                float fLife;
                float fWidth;
                

                b.GetBarkData( p.row , p.col , out fPos , out fCenter , out fDir , out fNor , out fTang,  out fLife,out fWidth);

                    Vector3 x = Random.onUnitSphere;
                for( int j = 0;  j < 4; j ++ ){

                    Vector3 left = Vector3.Cross( fNor , x );

                    Vector2 uv2 = new Vector2( j/2 , j%2 );
                    //float
                    
                    Vector3 ffPos = p.offset * fNor  * offsetSize + fPos +  left *(uv2.x - .5f) * flowerSize  + fNor  * (uv2.y-.5f) * flowerSize;

            
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 0 ] = ffPos.x;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 1 ] = ffPos.y;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 2 ] = ffPos.z;

                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 3 ] = fNor.x;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 4 ] = fNor.y;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 5 ] = fNor.z;

                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 6 ] = fCenter.x;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 7 ] = fCenter.y;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 8 ] = fCenter.z;
                    
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 9 ] = uv2.x;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 10 ] = uv2.y;

                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 11 ] = baseVal;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 12 ] = totalPoints;       
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 13 ] = fLife/maxTime;

                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 14 ] = p.offset;
                    flowerVals[  ( baseFlowerID * 4 + j) * 16 + 15 ] = p.row;
                    
                   

                }



                // Adding the trisss
                flowerTris[baseFlowerID * 6 + 0] = baseFlowerID * 4 + 0;
                flowerTris[baseFlowerID * 6 + 1] = baseFlowerID * 4 + 1;
                flowerTris[baseFlowerID * 6 + 2] = baseFlowerID * 4 + 3;
                flowerTris[baseFlowerID * 6 + 3] = baseFlowerID * 4 + 0;
                flowerTris[baseFlowerID * 6 + 4] = baseFlowerID * 4 + 3;
                flowerTris[baseFlowerID * 6 + 5] = baseFlowerID * 4 + 2;


                 
                baseFlowerID ++;


            }

     
          /*  foreach( BranchPoint p in b.points ){
                
                vals[ id  * 16 + 0] = p.position.x;
                vals[ id  * 16 + 1] = p.position.y;
                vals[ id  * 16 + 2] = p.position.z;
                vals[ id  * 16 + 3] = p.normal.x;
                vals[ id  * 16 + 4] = p.normal.y;
                vals[ id  * 16 + 5] = p.normal.z;
                vals[ id  * 16 + 6] = p.tangent.x;
                vals[ id  * 16 + 7] = p.tangent.y;
                vals[ id  * 16 + 8] = p.tangent.z;
                
                vals[ id  * 16 + 9] = p.positionInBranch;
                vals[ id  * 16 + 10] = p.timeCreated;
                vals[ id  * 16 + 11] = p.timeCreated/maxTime;
                vals[ id  * 16 + 12] = 0;
                vals[ id  * 16 + 13] = 0;
                vals[ id  * 16 + 14] = 0;
                vals[ id  * 16 + 15] = 0;

                id ++;
            }*/

            branchID ++;
        }

     /*   skeletonBuffer = new ComputeBuffer( totalPoints , 16 * sizeof(float));
        skeletonBuffer.SetData(vals);


        barkBuffer = new ComputeBuffer( totalBarkPoints ,16 * sizeof(float));
        barkBuffer.SetData(barkVals);



        flowerBuffer = new ComputeBuffer( totalFlowerPoints ,16 * sizeof(float) );
        flowerBuffer.SetData(flowerVals);


        barkTriBuffer = new ComputeBuffer( totalBarkTris , sizeof(int));
        barkTriBuffer.SetData(barkTris);*/


        mesh = new Mesh();

        mesh.vertices = barkPositions;
        mesh.tangents = barkTangents;
        mesh.normals = barkNormals;
        mesh.uv = barkUVs;
        mesh.colors = barkColors;
        mesh.triangles = barkTris;

        mesh.indexFormat = IndexFormat.UInt32;
        meshFilter.mesh = mesh;



RebuildMeshFromData( barkVals , barkTris, flowerVals , flowerTris );
     










    }






    public void SetGrowthValue(float v){

    }

    void Update(){

        /*if( debugSkeleton ){

            if( skeletonMPB == null ){
                skeletonMPB = new MaterialPropertyBlock();
            }

            skeletonMPB.SetBuffer("_VertBuffer", skeletonBuffer);
            skeletonMPB.SetInt("_Count",totalPoints);
            skeletonMPB.SetFloat("_AmountShown", skeletonShown );
            skeletonMPB.SetMatrix("_World", transform.localToWorldMatrix );

            Graphics.DrawProcedural( skeletonMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalPoints * 3 * 3 , 1, null, skeletonMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
            
        }



        if( debugBark ){

            if( barkMPB == null ){
                barkMPB = new MaterialPropertyBlock();
            }

            barkMPB.SetBuffer("_VertBuffer", barkBuffer);
            barkMPB.SetInt("_Count",totalBarkPoints);
            barkMPB.SetFloat("_AmountShown", barkShown );
            barkMPB.SetMatrix("_World", transform.localToWorldMatrix );

        //Graphics.DrawProcedural( barkMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalBarkPoints * 6 , 1, null, barkMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
        
        }


        if( debugMesh ){

            if( meshMPB == null ){
                meshMPB = new MaterialPropertyBlock();
            }

            meshMPB.SetBuffer("_VertBuffer", barkBuffer);
            meshMPB.SetBuffer("_TriBuffer", barkTriBuffer);
            meshMPB.SetInt("_Count",totalBarkPoints);
            meshMPB.SetFloat("_AmountShown", barkShown );
            
            meshMPB.SetMatrix("_World", transform.localToWorldMatrix );

            Graphics.DrawProcedural( meshMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalBarkTris , 1, null, meshMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
            
        }


        if( debugFlower ){

            if( flowerMPB == null ){
                flowerMPB = new MaterialPropertyBlock();
            }

            flowerMPB.SetBuffer("_VertBuffer", flowerBuffer);
            flowerMPB.SetInt("_Count",totalFlowerPoints);
            flowerMPB.SetFloat("_AmountShown", flowersShown );
            flowerMPB.SetFloat("_FallingAmount", flowersFallen );
            
            flowerMPB.SetMatrix("_World", transform.localToWorldMatrix );

            Graphics.DrawProcedural( flowerMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalFlowerPoints * 3 * 2 , 1, null, flowerMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
            
        }*/


    }

    public void SaveTree( string name , float[] verts , int[] tris , float[] flowers ){

        string saveName_Verts = Application.streamingAssetsPath + "Trees/" + name + ".treeVert";
        string saveName_Tris = Application.streamingAssetsPath + "Trees/" + name + ".treeTri";
        string saveName_Flowers = Application.streamingAssetsPath + "Trees/" + name + ".treeFlower";


        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(saveName_Verts,FileMode.Create);
        bf.Serialize(stream,verts);
        stream.Close();



        stream = new FileStream(saveName_Tris,FileMode.Create);
        bf.Serialize(stream,tris);
        stream.Close();

        
        stream = new FileStream(saveName_Flowers,FileMode.Create);
        bf.Serialize(stream,flowers);
        stream.Close();

    }

    public void LoadTree(string name){
        
        string saveName_Verts = Application.streamingAssetsPath + "Trees/" + name + ".treeVert";
        string saveName_Tris = Application.streamingAssetsPath + "Trees/" + name + ".treeTri";
        string saveName_Flowers = Application.streamingAssetsPath + "Trees/" + name + ".treeFlower";
        string saveName_FlowersTris = Application.streamingAssetsPath + "Trees/" + name + ".treeFlowerVert";
 
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(saveName_Verts,FileMode.Open);
        float[] data_Verts = bf.Deserialize(stream) as float[];
        stream.Close();

        stream = new FileStream(saveName_Tris,FileMode.Open);
        int[] data_Tris = bf.Deserialize(stream) as int[];
        stream.Close();

        stream = new FileStream(saveName_Flowers,FileMode.Open);
        float[] data_Flowers = bf.Deserialize(stream) as float[];
        stream.Close();

        stream = new FileStream(saveName_FlowersTris,FileMode.Open);
        int[] data_FlowersTris = bf.Deserialize(stream) as int[];
        stream.Close();


        RebuildMeshFromData( data_Verts, data_Tris , data_Flowers, data_FlowersTris );


    }


    public void RebuildMeshFromData( float[] data_Verts , int[] data_Tris , float[] data_Flowers , int[] data_FlowersTris ){


        // our verts we dont need to recreate
        // but our flowers we DO ( 1 quad per flower! )
        int total = (data_Verts.Length / 16) + (data_Flowers.Length/16);
     
        mesh = new Mesh();

        Vector3[] verts = new Vector3[total];
        Vector3[] normals = new Vector3[total];
        Vector3[] tangents = new Vector3[total];
        Vector2[] uvs = new Vector2[ total ];
        Vector3[] data1 = new Vector3[ total ];
        Vector2[] data2 = new Vector2[ total ];

        int index = 0;
        for( int i = 0; i < data_Verts.Length/16; i++ ){

            verts[index] = new Vector3(     data_Verts[index * 16  + 0],
                                            data_Verts[index * 16  + 1],
                                            data_Verts[index * 16  + 2] );

            normals[index] = new Vector3(   data_Verts[index * 16  + 3],
                                            data_Verts[index * 16  + 4],
                                            data_Verts[index * 16  + 5] );

            tangents[index] = new Vector3(  data_Verts[index * 16  + 6],
                                            data_Verts[index * 16  + 7],
                                            data_Verts[index * 16  + 8] );
            
            uvs[index]      = new Vector2(  data_Verts[index * 16  + 9],
                                            data_Verts[index * 16  + 10] );

            data1[index]    = new Vector3(  data_Verts[index * 16  + 11],
                                            data_Verts[index * 16  + 12],
                                            data_Verts[index * 16  + 13] );

            data2[index]    = new Vector2(  data_Verts[index * 16  + 14],
                                            data_Verts[index * 16  + 15] );

            index ++;

        }

    

        int baseIndex = index;


        print( baseIndex );
        print( data_Flowers.Length/16);
        print(total);

        print( baseIndex + data_Flowers.Length/16);
        for( int i = 0; i < data_Flowers.Length/16; i++ ){

            verts[index] = new Vector3(     data_Flowers[i * 16  + 0],
                                            data_Flowers[i * 16  + 1],
                                            data_Flowers[i * 16  + 2] );

            normals[index] = new Vector3(   data_Flowers[i * 16  + 3],
                                            data_Flowers[i * 16  + 4],
                                            data_Flowers[i * 16  + 5] );

            tangents[index] = new Vector3(  data_Flowers[i * 16  + 6],
                                            data_Flowers[i * 16  + 7],
                                            data_Flowers[i * 16  + 8] );
        
            uvs[index]      = new Vector2(  data_Flowers[i * 16  + 9],
                                            data_Flowers[i * 16  + 10] );

            data1[index]    = new Vector3(  data_Flowers[i * 16  + 11],
                                            data_Flowers[i * 16  + 12],
                                            data_Flowers[i * 16  + 13] );

            data2[index]    = new Vector2(  data_Flowers[i * 16  + 14],
                                            data_Flowers[i * 16  + 15] );

            index ++;
        }


        // updating our triangle pointers
        for( int i = 0; i < data_FlowersTris.Length; i++ ){
            data_FlowersTris[i] = data_FlowersTris[i] + baseIndex;
        }


    

        mesh.vertices = verts;
        mesh.normals  = normals;
        mesh.uv = uvs;
        //mesh.triangles = new int[ data_Tris.Length + data_FlowersTris.Length ];

        mesh.SetUVs(1 ,tangents);
        mesh.SetUVs(2 ,data1);
        mesh.SetUVs(3 ,data2);

        mesh.indexFormat = IndexFormat.UInt32;
        mesh.subMeshCount = 2;
        mesh.SetTriangles( data_Tris , 0 );
        mesh.SetTriangles( data_FlowersTris , 1 );


        meshFilter.mesh = mesh;


    }

    public void LoadTree(){

    }

}

