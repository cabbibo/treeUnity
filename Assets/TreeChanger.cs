using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class TreeChanger : MonoBehaviour
{

    public float changeSpeed;
    public float lastChangeTime;
    public Tree tree;


public void OnEnable(){
    lastChangeTime = 0;
}
    // Update is called once per frame
    void Update()
    {


        print( Time.time - lastChangeTime);

        float v =  (Time.time - lastChangeTime) / changeSpeed;


        if( tree.barkMPB != null ){
            tree.barkMPB.SetFloat("_CurrTime" , Mathf.Min( v *2 , 1-v)*1.5f );
        }

        if( tree.enabled == false ){
            tree.enabled = true;
        }
        if( Time.time - lastChangeTime > changeSpeed){
            tree.enabled = false;
            lastChangeTime = Time.time;
            
        }
    
    }

    
   // Updates in Edit Mode!
   void OnDrawGizmos()
   {
 
      #if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying  )
            {
        
               UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
               UnityEditor.SceneView.RepaintAll();
            }
      #endif

   }

}


