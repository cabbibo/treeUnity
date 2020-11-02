using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;


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


    


    

    // Where we want branches to start and end
    [Range(0,1)] public float startBranchLocation = .5f;
    [Range(0,2)] public float startBranchReducer = .9f;
    [Range(0,1)] public float endBranchLocation = 1;
    [Range(0,1)] public float endOfBranchWeight = .5f;
    [Range(0,2)] public float endOfBranchWeightReducer = .9f;








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

    

    public int currentTotalPoints;
    public int currentTotalBranches;

    public List<Branch> branches;//<Branches>

  


    [Header("Rendering")]


    public bool debugSkeleton;
    public ComputeBuffer skeletonBuffer;
    public Material skeletonMaterial;


    public bool debugBark;
    public bool debugMesh;
    public ComputeBuffer barkBuffer;
    public ComputeBuffer barkTriBuffer;
    public Material barkMaterial;
    public Material meshMaterial;



    [Range(0,1)]
    public float barkShown;
    
    [Range(0,1)]
    public float skeletonShown;



    /*

        Todo Variables


    */

        

    [Range(0,1)] public float upDesire = 1;
    
    [Range(0,2)] public float upDesireReducer = .9f;
    //public FastNoise noise;
    

    public Vector3 offset;
    public void OnEnable(){
        offset = new Vector3( 0 , Mathf.Sin(Time.time * 10000) * 1000 ,0 );
      //  noise = new FastNoise(); // Create a FastNoise object
      //  noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);

        BuildBranches();
    }
    
    public void BuildBranches(){

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
    public void BuildMesh(){        


        float maxTime = 0;
        totalPoints = 0;


        
        totalBarkPoints = 0;
        totalBarkTris = 0;
        foreach( Branch b in branches ){
            
            totalBarkPoints += b.numBarkRows * b.numBarkColumns;

            totalBarkTris +=  (b.numBarkRows-1) * (b.numBarkColumns-1) * 3 * 2;
        
            for( int i = 0; i < b.points.Count; i++ ){
                maxTime = Mathf.Max(maxTime , b.points[i].timeCreated);

                totalPoints ++;
            }
        
        
        }


        float[] barkVals = new float[ totalBarkPoints * 12 ];
        float[] vals = new float[ totalPoints  * 16 ];


        int[] barkTris = new int[totalBarkTris]; 


        int id = 0;
        int branchID = 0;
        int baseBarkID =0;
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
                    float noramlizedColID = (float)j/((float)b.numBarkColumns-1);

                    float v = normalizedRowID * .99f * ((float)totalPoints-1);
                
                    float up = Mathf.Ceil(v);
                    float down = Mathf.Floor(v);
                    float inVal = v - down;

                    Vector3 fPos;
                    Vector3 fPos1;


                    float fLife;
                    float fWidth;
                    
                    if( inVal == 0 || up == down ){

                        v += .0001f;
                        up = Mathf.Ceil(v);
                        down = Mathf.Floor(v);
                        inVal = v - down;

                    }
                    
         
                    Branch.Point p1 = b.points[(int)down];
                    Branch.Point p2 = b.points[(int)up];

                    fPos = cubicPoint( inVal , p1.position , p1.position + p1.normal / 3 , p2.position - p2.normal/3 , p2.position );
                    fPos1 = cubicPoint( inVal + .001f , p1.position , p1.position + p1.normal / 3 , p2.position - p2.normal/3 , p2.position );

                    fLife = Mathf.Lerp( p1.timeCreated , p2.timeCreated, inVal);
                    fWidth = Mathf.Lerp( p1.width , p2.width, inVal);
                    Vector3 fNor = Vector3.Lerp( p1.normal , p2.normal, inVal);
                    Vector3 fTan = Vector3.Lerp( p1.tangent , p2.tangent, inVal);
                    Vector3 fBi = Vector3.Lerp( p1.binormal , p2.binormal, inVal);


                    float angle = noramlizedColID * 2 * Mathf.PI;
                    float radius = .02f * (1-normalizedRowID)/ (1+fLife); 

                    Vector3 outVec = (fTan * Mathf.Sin( angle )  - fBi * Mathf.Cos(angle)) * fWidth;
                    fPos += outVec;// radius;;

                    outVec = outVec.normalized;
                    
        
                    // TODO MAKE THIS 
                    barkVals[ baseBarkID * 12 + 0 ] = fPos.x;
                    barkVals[ baseBarkID * 12 + 1 ] = fPos.y;
                    barkVals[ baseBarkID * 12 + 2 ] = fPos.z;

                    barkVals[ baseBarkID * 12 + 3 ] = outVec.x;
                    barkVals[ baseBarkID * 12 + 4 ] = outVec.y;
                    barkVals[ baseBarkID * 12 + 5 ] = outVec.z;
                    
                    barkVals[ baseBarkID * 12 + 6 ] = noramlizedColID;
                    barkVals[ baseBarkID * 12 + 7 ] = normalizedRowID;
                    barkVals[ baseBarkID * 12 + 8 ] = baseVal;
                    barkVals[ baseBarkID * 12 + 9 ] = totalPoints;
                    barkVals[ baseBarkID * 12 + 10 ] = fLife/maxTime;
                    barkVals[ baseBarkID * 12 + 11 ] = 0;

                 
                    baseBarkID ++;
                }


            }



            

     
            foreach( Branch.Point p in b.points ){
                
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
            }

            branchID ++;
        }

        skeletonBuffer = new ComputeBuffer( totalPoints , 16 * sizeof(float));
        skeletonBuffer.SetData(vals);


        barkBuffer = new ComputeBuffer( totalBarkPoints ,12 * sizeof(float));
        barkBuffer.SetData(barkVals);


        barkTriBuffer = new ComputeBuffer( totalBarkTris , sizeof(int));
        barkTriBuffer.SetData(barkTris);



    }






    public void SetGrowthValue(float v){

    }

    public MaterialPropertyBlock skeletonMPB;
    public MaterialPropertyBlock barkMPB;
    public MaterialPropertyBlock meshMPB;
    void Update(){

        if( debugSkeleton ){

        if( skeletonMPB == null ){
            skeletonMPB = new MaterialPropertyBlock();
        }

        skeletonMPB.SetBuffer("_VertBuffer", skeletonBuffer);
        skeletonMPB.SetInt("_Count",totalPoints);
        skeletonMPB.SetFloat("_AmountShown", skeletonShown );

        Graphics.DrawProcedural( skeletonMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalPoints * 3 * 3 , 1, null, skeletonMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
        
        }



        if( debugBark ){

        if( barkMPB == null ){
            barkMPB = new MaterialPropertyBlock();
        }

        barkMPB.SetBuffer("_VertBuffer", barkBuffer);
        barkMPB.SetInt("_Count",totalBarkPoints);
        barkMPB.SetFloat("_AmountShown", barkShown );

        Graphics.DrawProcedural( barkMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalBarkPoints * 6 , 1, null, barkMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
        
        }


        if( debugMesh ){

            if( meshMPB == null ){
                meshMPB = new MaterialPropertyBlock();
            }

            meshMPB.SetBuffer("_VertBuffer", barkBuffer);
            meshMPB.SetBuffer("_TriBuffer", barkTriBuffer);
            meshMPB.SetInt("_Count",totalBarkPoints);
            meshMPB.SetFloat("_AmountShown", barkShown );

            Graphics.DrawProcedural( meshMaterial ,  new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles,totalBarkTris , 1, null, meshMPB, ShadowCastingMode.On, true, LayerMask.NameToLayer("Default"));
            
        }


    }


        Vector3 cubicPoint( float v , Vector3 p1 , Vector3 p2 , Vector3 p3 , Vector3 p4 ){
            float c = 1.0f - v;

            float w1 = c*c*c;
            float w2 = 3*v*c*c;
            float w3 = 3*v*v*c;
            float w4 = v*v*v;

            return p1 * w1 + p2 * w2 + p3 * w3 + p4 * w4;

        }




}



public class Branch{
        public Tree tree;

        public Vector3 direction;

        public Vector3 startPosition;
        public Vector3 endPosition;

        public float length;

        public Branch parent;
        public List<Branch> children;
        public List<Point> points;
        public float pointAlongParent;

        public float baseWidth;
        public float endWidth;

        public float timeCreated;
        public float positionInBranch;

        public List<Flower> flowers;
        public int numPoints;

        public int iterationLevel;


        public int numBarkColumns;
        public int numBarkRows;

        public struct Point{
            public Vector3 position;
            public float timeCreated;
            public float positionInBranch;
            public Vector3 tangent;
            public Vector3 normal;

            public Vector3 binormal;

           public float width;

            public Point(Vector3 pos , float p , float t , float w ){
                this.timeCreated = t;
                this.positionInBranch = p;
                this.position = pos;
                this.tangent = Vector3.one;
                this.normal = Vector3.one;
                this.binormal  = Vector3.one;
                this.width = w;
            }

        }

        Vector3 GetPositionAlongPoints(float val , out float w){

            // Reduce by tiny amount so we can still sample up!
            // Also it gives us less of a chance of hitting the points exactly
            float v = val * .99f * ((float)numPoints-1);
        
            float up = Mathf.Ceil(v);
            float down = Mathf.Floor(v);
            float inVal = v - down;

            Vector3 fPos;

                    
            if( inVal == 0 || up == down ){
            
                fPos = points[(int)down].position;
                w = points[(int)down].width;
            
            }else{

                Branch.Point p1 = points[(int)down];
                Branch.Point p2 = points[(int)up];
                fPos = cubicPoint( inVal , p1.position , p1.position + p1.normal / 3 , p2.position - p2.normal/3 , p2.position );
                w = Mathf.Lerp( p1.width , p2.width , inVal);
            }

            return fPos;

        }

        public Branch( Tree t, int IL , Branch par , Vector3 startPos , Vector3 dir , float posInBranch , float tCreated , float currentLength , float width ){
            
            iterationLevel = IL;

            parent = par;
            tree = t;
            startPosition = startPos;
            direction = dir;

            positionInBranch = posInBranch;
            timeCreated = tCreated;

            baseWidth = width * tree.widthReducer;;

        
            numBarkRows = currVal( tree.numBarkRows, tree.numBarkRowsReducer);
            numBarkColumns = currVal( tree.numBarkColumns, tree.numBarkColumnsReducer);

            children = new List<Branch>();
            flowers = new List<Flower>();
            points = new List<Point>();

            // Figures out if we are using the base length wheteher we are inheriting or using from base
            float parentLengthMax = currVal( tree.parentLengthMax , tree.parentLengthMaxReducer );
            length = Mathf.Lerp(  currVal( tree.length , tree.lengthReducer ) , currentLength  , parentLengthMax );
            float lengthVariation = currVal( tree.lengthVariation , tree.lengthVariationReducer );
            length *= Random.Range( 1 , 1 - lengthVariation);

            float baseVsTipLength = currVal( tree.baseVsTipLength , tree.baseVsTipLengthReducer );
            length *= Mathf.Lerp( 1 , 1-posInBranch , baseVsTipLength);

            numPoints = currVal( tree.pointsPerBranch , tree.pointsPerBranchReducer );
            endPosition = startPos + dir * length;;
          
            MakePoints();

            tree.branches.Add(this);
            tree.currentTotalBranches += 1;

            if( iterationLevel < tree.maxIterationLevel ){
                MakeChildren();
            }


        }



        // Making points along each branch
        public void MakePoints(){
        

            Vector3 currPos = startPosition;

            

                tree.currentTotalPoints += numPoints;

      
                
           
            // place the points along the branch
            for( int i = 0; i  < numPoints; i++ ){

                float valInBranch = ((float)i/((float)numPoints-1));

                float widthMultiplier = 1;
                if( iterationLevel == 0 ){
                    widthMultiplier = tree.trunkCurve.Evaluate( valInBranch );
                }else{
                    widthMultiplier = tree.branchCurve.Evaluate( valInBranch );
                }




                if( i != 0 ){

                    float currNoiseSize = currVal( tree.noiseSize , tree.noiseSizeReducer );
                    float currNoisePower = currVal( tree.noisePower , tree.noisePowerReducer );


                    float currUp = currVal( tree.upDesire , tree.upDesireReducer );

                    if( i != 1 ){

                        Vector3 dir = points[i-1].position - points[i-2].position;
                        currPos += dir.normalized * length  * ((float)1/((float)numPoints-1));  
                    }else{

                        currPos += length * direction * ((float)1/((float)numPoints-1));  
                    }
                    

                    currPos += currUp * Vector3.up * .003f;


                   // Vector3 addVal =  Vector3.Cross( Random.insideUnitSphere, direction.normalized ).normalized;
                    Vector3 noiseDir = Perlin.CurlNoise(  currPos  * currNoiseSize + tree.offset );

                    currPos += noiseDir  * currNoisePower * .1f;


                    
                    
                   // currPos += addVal * .01f * currNoiseSize * valInBranch * currNoise;
                }

                float fWidth = baseWidth * widthMultiplier;
                Point p = new Point( currPos , valInBranch , timeCreated + valInBranch , fWidth ); 
                points.Add( p );
                // TODO ADD NOISE
            }

            // Gets Tangents for each of the points for sake of
            // cubic beziers
            for( int i = 0; i < numPoints; i++ ){

                Branch.Point p = points[i];

                if( i == 0 ){
                    p.normal = (points[1].position - p.position);
                }else if( i == points.Count-1 ){
                    p.normal = (p.position - points[points.Count-2].position);
                }else{
                    p.normal = -(points[i-1].position - points[i +1].position);
                }


                if( i == 0 ){
                    p.tangent = (Vector3.Cross( p.normal.normalized , Vector3.left )).normalized;
                    p.binormal = (Vector3.Cross( p.normal , p.tangent )).normalized;
                }else{
                    p.tangent = -(Vector3.Cross( p.normal.normalized , points[i-1].binormal )).normalized;
                    p.binormal = (Vector3.Cross( p.normal , p.tangent )).normalized;
                }

                points[i] = p;
            
            
            }

        }


        public void  MakeChildren(){


          
            int currMaxBranches = currVal( tree.maxNumberBranchesPerBranch, tree.maxNumberBranchesPerBranchReducer);
            float currChance = currVal( tree.branchChance, tree.branchChanceReducer  );


            for( int i = 0;  i < currMaxBranches; i++ ){
                
                float chance = Random.Range(0f,1f);
                if( chance < currChance ){

                
                    float pointAlongPath = Random.Range(0,1f);;
                    pointAlongPath *= pointAlongPath;

                    pointAlongPath = 1-pointAlongPath;
                 
                    float width;
            
                    Vector3 startDir = GetPositionAlongPoints(pointAlongPath + .01f , out width);
                            // TODO this needs to be from start to end point not full along
                    Vector3 startPosition = GetPositionAlongPoints( pointAlongPath , out width);
                    
                 
                    startDir -= startPosition;
                    startDir= startDir.normalized;

                    Vector3 addVal =  Vector3.Cross( Random.insideUnitSphere, direction.normalized ).normalized;


                    Vector3 newDir = Vector3.Lerp( startDir , addVal , Random.Range(tree.minAngleMatch, tree.maxAngleMatch) );

                    Vector3 startDirection =newDir;

                    if( width > tree.widthCutoff  && tree.branches.Count < tree.maxBranches && tree.currentTotalPoints < tree.maxPoints ){
                        children.Add( new Branch( tree , iterationLevel + 1 , this  , startPosition , startDirection , pointAlongPath , timeCreated + pointAlongPath , length , width ));
                    }

                }


            }
        }

        private void print(string s){
            Debug.Log(s);
        }


        float currVal( float val , float reducer){
            return val * Mathf.Pow( reducer , iterationLevel );          
        }

        int currVal( int val , float reducer ){
            return (int)Mathf.Ceil((float)val * Mathf.Pow( reducer, iterationLevel));
        }




   


        Vector3 cubicPoint( float v , Vector3 p1 , Vector3 p2 , Vector3 p3 , Vector3 p4 ){
            	float c = 1.0f - v;

                float w1 = c*c*c;
                float w2 = 3*v*c*c;
                float w3 = 3*v*v*c;
                float w4 = v*v*v;

                return p1 * w1 + p2 * w2 + p3 * w3 + p4 * w4;

        }


        
        float cubicPoint( float v , float p1 , float p2 , float p3 , float p4 ){
            	float c = 1.0f - v;

                float w1 = c*c*c;
                float w2 = 3*v*c*c;
                float w3 = 3*v*v*c;
                float w4 = v*v*v;

                return p1 * w1 + p2 * w2 + p3 * w3 + p4 * w4;

        }

}



public class Flower{
    public Branch parent;
    public float pointAlongParent;

}

