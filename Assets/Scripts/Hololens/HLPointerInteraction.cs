using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class HLPointerInteraction : IMixedRealityPointerHandler, IMixedRealityFocusHandler
{

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData.Count, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("PointerClicked " + eventData.Pointer.Result, LogLevel.Display, LogMessageCategory.VR);
       
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
    {

        CCDebug.Log(eventData, LogLevel.Display, LogMessageCategory.VR);

    }

    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }
}
