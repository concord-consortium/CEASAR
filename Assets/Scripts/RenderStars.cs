using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderStars : MonoBehaviour
{
    public GameObject star;
    // Start is called before the first frame update
    void Start()
    {
        renderStars();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void renderStars()
    {
        if (star != null)
        {
            for (int i = 0; i < 1000; i++) {
                Vector3 newPos = Random.onUnitSphere * 50;
 
                GameObject starObject = Instantiate(star, newPos, transform.rotation);
                starObject.transform.LookAt(transform);
            }
       }
    }

}
