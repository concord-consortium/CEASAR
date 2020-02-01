using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCamera : MonoBehaviour
{
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void ZoomOut()
    {
        anim.SetBool("IsZoomedOut", true);
    }
    public void ZoomIn()
    {
        anim.SetBool("IsZoomedOut", false);
    }
}
