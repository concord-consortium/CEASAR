﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFaceCamera : MonoBehaviour
{
    private Camera mainCam;

    private void OnDisable()
    {
        mainCam = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mainCam)
        {
            mainCam = Camera.main;
        }

        if (mainCam)
        {
            transform.LookAt(mainCam.transform);
        }
    }
}
