using System;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class HLPointerInteraction : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler
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

    void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);
    }

    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityFocusHandler>(this);
    }

    /*void Update()
    {
        foreach(var source in CoreServices.InputSystem.DetectedInputSources)
        {
            // Ignore anything that is not a hand because we want articulated hands
            if (source.SourceType == Microsoft.MixedReality.Toolkit.Input.InputSourceType.Hand)
            {
                foreach (var p in source.Pointers)
                {
                    if (p is IMixedRealityNearPointer)
                    {
                        // Ignore near pointers, we only want the rays
                        continue;
                    }
                    if (p.Result != null)
                    {
                        var startPoint = p.Position;
                        var endPoint = p.Result.Details.Point;
                        var hitObject = p.Result.Details.Object;
                        if (hitObject)
                        {
                            Debug.Log(hitObject.name);
                        }
                        else
                        {
                            Debug.Log(endPoint);
                        }
                    }

                }
            }
        }
    }*/

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        // detectClick(eventData);
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        detectClick(eventData);

    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log(eventData, LogLevel.Verbose, LogMessageCategory.VR);
    }

    void detectClick(MixedRealityPointerEventData eventData)
    {
        CCDebug.Log("PointerClicked " + eventData.Pointer.Result, LogLevel.Display, LogMessageCategory.VR);
        var pos = eventData.Pointer.Result.Details.Point;
        if (!mainUIController) mainUIController = FindObjectOfType<MenuController>();
        if (mainUIController)
        {
            if (mainUIController.IsDrawing && Vector3.Magnitude(pos) > 100f)
            {
                addAnnotation(pos);
            }
            else
            {
                var hitObject = eventData.Pointer.Result.Details.Object;
                if (hitObject)
                {
                    StarComponent sc = hitObject.GetComponent<StarComponent>();
                    MoonComponent mc = hitObject.GetComponent<MoonComponent>();
                    SunComponent suc = hitObject.GetComponent<SunComponent>();
                    FloatingInfoPanel fp = hitObject.GetComponent<FloatingInfoPanel>();
                    if (sc) sc.HandleSelectStar(true);
                    if (mc) mc.HandleSelectMoon(true);
                    if (suc) suc.HandleSelectSun(true);
                    if (fp) fp.HandleClose();
                }
            }
        }
    }

    void addAnnotation(Vector3 pos)
    {
        float r = SimulationManager.Instance.SceneRadius + 2f;

        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();

        if (mainUIController && mainUIController.IsDrawing && annotationTool)
        {
            // point outwards for annotating
            annotationTool.Annotate(Vector3.ClampMagnitude(pos, r));
        }
    }
    public void OnFocusEnter(FocusEventData eventData)
    {
       // throw new System.NotImplementedException();
       var hitObject = eventData.Pointer.Result.Details.Object;
       if (hitObject)
       {
           StarComponent sc = hitObject.GetComponent<StarComponent>();
           if (sc) sc.CursorHighlightStar(true);
       }
    }

    public void OnFocusExit(FocusEventData eventData)
    {
       // throw new System.NotImplementedException();
       var hitObject = eventData.Pointer.Result.Details.Object;
       if (hitObject)
       {
           StarComponent sc = hitObject.GetComponent<StarComponent>();
           if (sc) sc.CursorHighlightStar(false);
       }
    }
}
