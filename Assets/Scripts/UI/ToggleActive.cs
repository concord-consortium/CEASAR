using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public bool isActiveByDefault = false;

    public KeyCode toggleKey;
    public KeyCode modifierKey = KeyCode.SysReq;
    
    private Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = this.GetComponent<Renderer>();
        renderer.enabled = isActiveByDefault;
    }

    // Update is called once per frame
    void Update()
    {
        if (modifierKey != KeyCode.SysReq)
        {
            // Optional second modifier - if set to something like Shift, then can only perform the toggle if shift
            // is held when pressing the key
            if (Input.GetKeyDown(toggleKey) && Input.GetKey(modifierKey))
            {
                renderer.enabled = !renderer.enabled;
            }
        } 
        else if (Input.GetKeyDown(toggleKey))
        {
            renderer.enabled = !renderer.enabled;
        }
    }
}
