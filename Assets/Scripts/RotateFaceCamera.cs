using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFaceCamera : MonoBehaviour
{
    private Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;   
    }

    // Update is called once per frame
    void Update()
    {
        if (!mainCam || mainCam.enabled == false)
        {
            mainCam = Camera.main;
        }

        if (mainCam)
        {
            transform.LookAt(mainCam.transform);
        }
    }
}
