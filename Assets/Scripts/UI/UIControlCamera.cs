using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControlCamera : MonoBehaviour
{
    Camera mainCam;
    public Transform cameraContainer;
    public bool enableControls = true;
    [SerializeField]
    private GameObject controlContainer;

    private bool orbitControlMode = false;
    private PlayerOrbitMoveControl orbitControls;
    public bool OrbitControlMode
    {
        get { return orbitControlMode; }
        set
        {
            orbitControlMode = value;
            if (orbitControlMode)
            {
                orbitControls = FindObjectOfType<PlayerOrbitMoveControl>();
            }
        }
    }
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
        if (enableControls)
        {
            if (controlContainer && !controlContainer.activeInHierarchy)
            {
                controlContainer.SetActive(true);
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Read the user input
                var x = Input.GetAxisRaw("Mouse X");
                var y = Input.GetAxisRaw("Mouse Y");
                LookUpDown(y * 100);
                LookLeftRight(x * 100);
            }
        }
        else if (controlContainer && controlContainer.activeInHierarchy)
        {
            controlContainer.SetActive(false);
        }
    }
    // rotate camera up/down
    public void LookUpDown(float speed)
    {
        if (enableControls)
        {
            if (orbitControlMode)
            {
                if (!orbitControls)
                {
                    orbitControls = FindObjectOfType<PlayerOrbitMoveControl>();
                }

                if (orbitControls)
                {
                    orbitControls.MoveCamera(0, speed * Time.deltaTime * -1, 0);
                }
            }
            else
            {
                Vector3 currentRotation = mainCam.transform.localEulerAngles;
                float xRotation = currentRotation.x;
                xRotation = xRotation + speed * Time.deltaTime * -1f;
                // values can be from 270 - 90
                float highAngle = 270f;
                float lowAngle = 90f;
                if (xRotation > lowAngle && xRotation < highAngle)
                    xRotation = currentRotation.x;
                mainCam.transform.localRotation = Quaternion.Euler(xRotation, currentRotation.y, currentRotation.z);
            }
        }
    }
    // Rotate parent left/right
    public void LookLeftRight(float speed)
    {
        if (enableControls)
        {
            if (orbitControlMode)
            {
                if (!orbitControls)
                {
                    orbitControls = FindObjectOfType<PlayerOrbitMoveControl>();
                }

                if (orbitControls)
                {
                    orbitControls.MoveCamera(speed * Time.deltaTime * -1, 0, 0);
                }
            }
            else
            {
                if (cameraContainer == null && mainCam.transform.parent)
                    cameraContainer = mainCam.transform.parent;
                else cameraContainer = mainCam.transform;
                cameraContainer.transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
            }
        }
    }
    public void ZoomOut()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == SimulationConstants.SCENE_STARS)
        {
            float zPos = mainCam.transform.position.z;
            if (zPos > -200f)
            {
                mainCam.transform.position = new Vector3(0, 0, zPos - 10f);
            }
        }
        else
        {
            float fov = mainCam.fieldOfView;
            if (fov < 75)
            {
                mainCam.fieldOfView = fov + 15f;
            }
        }
    }
    public void ZoomIn()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == SimulationConstants.SCENE_STARS)
        {
            float zPos = mainCam.transform.position.z;
            if (zPos < 0)
            {
                mainCam.transform.position = new Vector3(0, 0, zPos + 10f);
            }
        }
        else
        {
            float fov = mainCam.fieldOfView;
            if (fov > 45)
            {
                mainCam.fieldOfView = fov - 15f;
            }
        }
    }

}
