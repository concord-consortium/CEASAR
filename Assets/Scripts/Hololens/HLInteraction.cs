using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLInteraction : MonoBehaviour, IMixedRealityInputHandler, IMixedRealityPointerHandler, IMixedRealityTouchHandler
{
    void IMixedRealityInputHandler.OnInputDown(InputEventData eventData)
    {
        CCDebug.Log(eventData.InputSource, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("InputDown " + eventData.SourceId, LogLevel.Display, LogMessageCategory.VR);

    }

    void IMixedRealityInputHandler.OnInputUp(InputEventData eventData)
    {
        CCDebug.Log(eventData.InputSource, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("InputUp " + eventData.InputSource, LogLevel.Display, LogMessageCategory.VR);
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData.Count, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("PointerClicked " + eventData.Pointer.Result, LogLevel.Display, LogMessageCategory.VR);
       
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }

    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
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
        CCDebug.Log("TouchUpdated " + eventData.InputSource.Pointers[0].Position, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("TouchUpdated " + eventData.InputSource.Pointers[0].Position, LogLevel.Display, LogMessageCategory.VR);
        SimulationManager.Instance.InteractionControllerObject.GetComponent<InteractionController>().SetEarthLocationPin(eventData.InputSource.Pointers[0].Position);
    }
}
