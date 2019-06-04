using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControlCamera : MonoBehaviour
{
    Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LookUpDown(float speed)
    {
        mainCam.transform.Rotate(Vector3.left, speed * Time.deltaTime, Space.Self);
        // this.transform.Rotate(Vector3.down, rotateSpeed * Time.deltaTime);
    }
    public void LookLeftRight(float speed)
    {
        mainCam.transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
    }

}
