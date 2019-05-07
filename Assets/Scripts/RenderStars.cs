using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderStars : MonoBehaviour
{
    public GameObject star;
    // Start is called before the first frame update
    void Start()
    {
        if (star != null){
            Instantiate(star, transform.position, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
