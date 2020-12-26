using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class RotateAround : MonoBehaviour
{

    public Transform look;

    public float lookUp;
    public float up;
    public float radius;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3( Mathf.Sin(Time.time * speed)  * radius , up , -Mathf.Cos(Time.time * speed)  * radius  );
        transform.LookAt( look.position + Vector3.up * lookUp );
    }
}
