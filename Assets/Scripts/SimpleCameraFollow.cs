using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        if (target)
        {
            offset = transform.position - target.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            transform.position = target.transform.position - offset;
            transform.LookAt(target);
        }
    }
}
