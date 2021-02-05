using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLTouchInteraction : MonoBehaviour, IMixedRealityInputHandler, IMixedRealityTouchHandler
{
    private StarComponent sc;
    void OnEnable()
    {
        sc = GetComponent<StarComponent>();
    }
    void IMixedRealityInputHandler.OnInputDown(InputEventData eventData)
    {
        if (sc)
        {
            CCDebug.Log(name, LogLevel.Display, LogMessageCategory.VR);
            if (sc)
            {
                sc.HandleSelectStar(true);
            }
        }

        // CCDebug.Log("InputDown " + eventData.SourceId, LogLevel.Display, LogMessageCategory.VR);

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
