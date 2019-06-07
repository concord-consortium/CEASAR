using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject indicator;
    Camera camera;
    RaycastHit hit;
    Ray ray;
    int layerMask;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        layerMask = LayerMask.GetMask("Earth");
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            // Do something with the object that was hit by the raycast.
            if (Input.GetMouseButtonDown(0))
            {
                if (indicator)
                {
                    GameObject indicatorObj = Instantiate(indicator, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                    StartCoroutine(selfDestruct(indicatorObj));
                }
            }
        }
    }

    IEnumerator selfDestruct(GameObject indicatorObj)
    {
        yield return new WaitForSeconds(3.0f);
        if (indicatorObj)
        {
            Destroy(indicatorObj);
        }
    }

}
