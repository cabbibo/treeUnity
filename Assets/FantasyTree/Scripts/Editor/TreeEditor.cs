using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Tree))]
public class TreeEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        Tree tree = (Tree)target; 
        
        if(GUILayout.Button("Save Mesh")){
            tree.SaveMesh();
        }

        if(GUILayout.Button("Save Parameters")){
            tree.SaveParameters();
        }

        if(GUILayout.Button("Load Parameters")){
            tree.LoadParameters();
        }

        if(GUILayout.Button("Load Mesh")){
            tree.LoadMesh();
        }


        if(GUILayout.Button("Rebuild")){
            tree.BuildBranches();
        }

        


        DrawDefaultInspector();
    }
}