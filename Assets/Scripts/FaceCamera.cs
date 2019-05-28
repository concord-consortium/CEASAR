using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Vector3 currentPosition;
    private Quaternion currentRotation;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = this.transform.position;
        currentRotation = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // if the scale of the parent changes, then so does the position
        if (currentPosition != this.transform.position || currentRotation != this.transform.rotation)
        {
            currentPosition = this.transform.position;
            currentRotation = this.transform.rotation;
            transform.LookAt(Camera.main.transform);
        }
    }
}
