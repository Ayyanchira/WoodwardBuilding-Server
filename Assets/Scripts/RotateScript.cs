using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{

    public Vector3 rotating;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotating * Time.deltaTime);
    }

    void Start()
    {
        transform.rotation = new Quaternion(45f,45f,45f,0);
    }

}