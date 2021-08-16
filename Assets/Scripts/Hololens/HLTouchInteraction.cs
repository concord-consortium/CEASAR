using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLTouchInteraction : MonoBehaviour, IMixedRealityInputHandler, IMixedRealityTouchHandler
{
    private StarComponent sc;
    private SunComponent suc;
    private MoonComponent mc;
    private FloatingInfoPanel fp;
    void OnEnable()
    {
        sc = GetComponent<StarComponent>();
        suc = GetComponent<SunComponent>();
        mc = GetComponent<MoonComponent>();
        fp = GetComponent<FloatingInfoPanel>();
    }
    void IMixedRealityInputHandler.OnInputDown(InputEventData eventData)
    {
        if (sc)
        {
            CCDebug.Log(name, LogLevel.Display, LogMessageCategory.VR);
            sc.HandleSelectStar(true);
        }

        // CCDebug.Log("InputDown " + eventData.SourceId, LogLevel.Display, LogMessageCategory.VR);
                    
        if (mc) mc.HandleSelectMoon();
        if (suc) suc.HandleSelectSun();
        if (fp) fp.HandleClose();
    }

    void IMixedRealityInputHandler.OnInputUp(InputEventData eventData)
    {
        CCDebug.Log(eventData.InputSource, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("InputUp " + eventData.InputSource, LogLevel.Display, LogMessageCategory.VR);
    }

    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
    {

        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("TouchCompleted " + eventData.InputData, LogLevel.Display, LogMessageCategory.VR);
       

    }

    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);

    }

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
    {
        Vector3 touchPosition = eventData.InputSource.Pointers[0].Position;
        SimulationManager.Instance.InteractionControllerObject.GetComponent<InteractionController>().SetEarthLocationPin(touchPosition);
    }
}
