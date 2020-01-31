using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControlCamera : MonoBehaviour
{
    Camera mainCam;
    public Transform cameraContainer;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;
        if (mainCam.transform.parent)
        {
            cameraContainer = mainCam.transform.parent;
        }
        else
        {
            cameraContainer = mainCam.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Read the user input
            var x = Input.GetAxisRaw("Mouse X");
            var y = Input.GetAxisRaw("Mouse Y");
            LookUpDown(y * 100);
            LookLeftRight(x * 100);
        }
    }
    // rotate camera up/down
    public void LookUpDown(float speed)
    {
        mainCam.transform.Rotate(Vector3.left, speed * Time.deltaTime, Space.Self);
        // this.transform.Rotate(Vector3.down, rotateSpeed * Time.deltaTime);
    }
    // Rotate parent left/right
    public void LookLeftRight(float speed)
    {
        if (cameraContainer == null && mainCam.transform.parent)
            cameraContainer = mainCam.transform.parent;
        cameraContainer.transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);

    }

}
