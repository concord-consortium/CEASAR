using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class HLPointerInteraction : MonoBehaviour, IMixedRealityPointerHandler
{
    SimulationManager manager;
    InteractionController interactionController;
    public AnnotationTool annotationTool;
    [SerializeField] MenuController mainUIController;
    private int layerMaskEarth;
    private int layerMaskStarsAnnotations;
    void Start()
    {
        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskStarsAnnotations = LayerMask.GetMask("Stars", "Annotations");
        manager = SimulationManager.Instance;
        interactionController = FindObjectOfType<InteractionController>();
        if (!mainUIController) mainUIController = FindObjectOfType<MenuController>();
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData.Count, LogLevel.Verbose, LogMessageCategory.VR);
        CCDebug.Log("PointerClicked " + eventData.Pointer.Result, LogLevel.Display, LogMessageCategory.VR);
        var pos = eventData.Pointer.Position;
        float r = SimulationManager.Instance.SceneRadius + 2f;
        if (mainUIController.IsDrawing && annotationTool)
        {
            annotationTool.Annotate(Vector3.ClampMagnitude(pos, r));
        }

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
