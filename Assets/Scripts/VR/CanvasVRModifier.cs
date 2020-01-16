using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasVRModifier : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        string model = UnityEngine.XR.XRDevice.model != null ? UnityEngine.XR.XRDevice.model : "";
        if (!string.IsNullOrEmpty(model))
        {
            GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
            OVRRaycaster OVRrc = null;
            if (gr != null)
            {
                Debug.Log("Found graphic raycaster");
                gr.enabled = false;
                OVRrc = gameObject.AddComponent<OVRRaycaster>();
                var vrInteraction = FindObjectOfType<VRInteraction>();
                if (vrInteraction)
                {
                    OVRrc.pointer = vrInteraction.GetComponent<VRInteraction>().vrPointerInstance;
                }
                
                OVRrc.blockingObjects = OVRRaycaster.BlockingObjects.All;
            }
        }
#endif
    }
}