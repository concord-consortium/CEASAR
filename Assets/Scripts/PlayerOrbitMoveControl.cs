using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbitMoveControl : MonoBehaviour
{
    
    public Transform target;
    private Transform avatar;
    public float distance = 20.0f;
    public float xSpeed = 40.0f;
    public float ySpeed = 80.0f;
 
    public float yMinLimit = -50f;
    public float yMaxLimit = 100f;
 
    public float distanceMin = 10f;
    public float distanceMax = 40f;

    private int layerMaskEarth;
    
    float x = 0.0f;
    float y = 0.0f;
    float d = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
        layerMaskEarth = LayerMask.GetMask("Earth");
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        d = distance;
        GameObject avatarObject = GameObject.FindGameObjectWithTag("LocalPlayerAvatar");
        if (avatarObject)
        {
            avatar = avatarObject.transform;
        }
    }

    void LateUpdate()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (target && (Input.GetKey(KeyCode.LeftShift)))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit, layerMaskEarth))
            {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // now get the avatar to look at the target
            if (avatar) avatar.LookAt(target);
            transform.rotation = rotation;
            transform.position = position;
        }
        if (target && (Input.GetKey(KeyCode.LeftControl)))
        {
            d -= Input.GetAxisRaw("Mouse Y");

            distance = Mathf.Clamp(d, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = transform.rotation * negDistance + target.position;

            transform.position = position;
        }
#endif
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
