/*==============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/
using UnityEngine;
using Vuforia;

/// <summary>
/// A default implementation of Model Reco Event Handler.
/// It registers itself at the ModelRecoBehaviour and is notified of new search results.
/// </summary>
public class BaseModelRecoEventHandler : MonoBehaviour, IObjectRecoEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    // ModelRecoBehaviour reference to avoid lookups
    ModelRecoBehaviour modelRecoBehaviour;
    ModelTargetBehaviour lastRecoModelTarget;
    bool searching;
    float lastStatusCheckTime;

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PROTECTED_MEMBER_VARIABLES

    // Target Finder reference to avoid lookups
    protected TargetFinder targetFinder;

    #endregion  // PROTECTED_MEMBER_VARIABLES


    #region PUBLIC_VARIABLES


    /// <summary>
    /// Can be set in the Unity inspector to tell Vuforia whether it should:
    /// - stop searching for new models, once a first model was found,
    ///   or:
    /// - continue searching for new models, even after a first model was found.
    /// </summary>
    [Tooltip("Whether Vuforia should stop searching for other models, after the first model was found.")]
    public bool StopSearchWhenModelFound;

    /// <summary>
    /// Can be set in the Unity inspector to tell Vuforia whether it should:
    /// - stop searching for new models, while a target is being tracked and is in view,
    ///   or:
    /// - continue searching for new models, even if a target is currently being tracked.
    /// </summary>
    [Tooltip("Whether Vuforia should stop searching for other models, while current model is tracked and visible.")]
    public bool StopSearchWhileTracking = true;//true by default, as this is the recommended behaviour

    #endregion // PUBLIC_VARIABLES



    #region UNTIY_MONOBEHAVIOUR_METHODS

    /// <summary>
    /// register for events at the ModelRecoBehaviour
    /// </summary>
    void Start()
    {
        // register this event handler at the model reco behaviour
        this.modelRecoBehaviour = GetComponent<ModelRecoBehaviour>();
        if (this.modelRecoBehaviour)
        {
            this.modelRecoBehaviour.RegisterEventHandler(this);
        }
    }

    void Update()
    {
        if (!VuforiaARController.Instance.HasStarted)
        {
            return;
        }

        if (this.targetFinder == null)
        {
            return;
        }

        // Check periodically if model target is tracked and in view
        float elapsed = Time.realtimeSinceStartup - this.lastStatusCheckTime;

        if (StopSearchWhileTracking && elapsed > 0.5f)
        {
            this.lastStatusCheckTime = Time.realtimeSinceStartup;

            if (IsModelTrackedInView(this.lastRecoModelTarget))
            {
                // Switch Model Reco OFF when model is being tracked/in-view
                if (this.searching)
                {
                    this.targetFinder.Stop();
                    this.searching = false;
                }
            }
            else
            {
                // Switch Mode Reco ON when no model is tracked/in-view
                if (!this.searching && this.modelRecoBehaviour.ModelRecoEnabled)
                {
                    this.targetFinder.StartRecognition();
                    this.searching = true;
                }
            }
        }
    }


    public void OnDestroy()
    {
        if (this.modelRecoBehaviour)
        {
            this.modelRecoBehaviour.UnregisterEventHandler(this);
        }

        this.modelRecoBehaviour = null;
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region IModelRecoEventHandler_IMPLEMENTATION

    /// <summary>
    /// called when TargetFinder has been initialized successfully
    /// </summary>
    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("ModelReco initialized.");

        // Keep a reference to the Target Finder
        this.targetFinder = targetFinder;
    }

    // Error callback methods implemented in CloudErrorHandler
    public void OnInitError(TargetFinder.InitState initError) { }
    public void OnUpdateError(TargetFinder.UpdateState updateError) { }

    /// <summary>
    /// when we start scanning, clear all trackables
    /// </summary>
    public void OnStateChanged(bool searching)
    {
        Debug.Log("<color=cyan>OnStateChanged() called: </color>" + (searching ? "searching" : "not searching"));

        this.searching = searching;

        if (searching)
        {
            // clear all known trackables
            if (this.targetFinder != null)
            {
                this.targetFinder.ClearTrackables();
            }
        }
    }

    /// <summary>
    /// Handles new search results.
    /// </summary>
    /// <param name="searchResult"></param>
    public virtual void OnNewSearchResult(TargetFinder.TargetSearchResult searchResult)
    {
        Debug.Log("<color=cyan>OnNewSearchResult() called: </color>" + searchResult.TargetName);

        // Find or create the referenced model target
        GameObject modelTargetGameObj = null;


        var existingModelTarget = FindExistingModelTarget(searchResult);

        if (existingModelTarget)
        {
            modelTargetGameObj = existingModelTarget.gameObject;
        }
        
        if (!modelTargetGameObj)
        {
            modelTargetGameObj = new GameObject("ModelTarget_" + searchResult.TargetName);
        }

        if (!modelTargetGameObj)
        {
            Debug.LogError("Could not create a Model Target.");
            return;
        }

        // Enable the new search result as a Model Target
        ModelTargetBehaviour mtb = targetFinder.EnableTracking(searchResult, modelTargetGameObj)
            as ModelTargetBehaviour;

        if (mtb)
        {
            this.lastRecoModelTarget = mtb;

            if (this.StopSearchWhenModelFound)
            {
                // Stop the target finder
                this.modelRecoBehaviour.ModelRecoEnabled = false;
            }
        }
    }

    #endregion // IModelRecoEventHandler_IMPLEMENTATION



    #region PRIVATE_METHODS

    ModelTargetBehaviour FindExistingModelTarget(TargetFinder.TargetSearchResult searchResult)
    {
        Debug.Log("<color=cyan>FindExistingModelTarget() called.</color>");

        var modelTargetsInScene = Resources.FindObjectsOfTypeAll<ModelTargetBehaviour>();

        if (modelTargetsInScene == null || modelTargetsInScene.Length == 0)
        {
            return null;
        }

        string targetName = searchResult.TargetName;

        foreach (var mt in modelTargetsInScene)
        {
            if (mt.TrackableName == targetName)
            {
                mt.gameObject.SetActive(true);
                return mt;
            }
        }

        return null;
    }

    GameObject CreateBoundingBox(string modelTargetName, OrientedBoundingBox3D bbox)
    {
        Debug.Log("<color=cyan>CreateBoundingBox() called.</color>");

        var bboxGameObj = new GameObject(modelTargetName + "_BoundingBox");
        bboxGameObj.transform.localPosition = bbox.Center;
        bboxGameObj.transform.localRotation = Quaternion.identity;
        bboxGameObj.transform.localScale = 2 * bbox.HalfExtents;
        bboxGameObj.AddComponent<BoundingBoxRenderer>();
        return bboxGameObj;
    }

    public static Bounds GetModelTargetWorldBounds(ModelTargetBehaviour mtb)
    {
        var bbox = mtb.ModelTarget.GetBoundingBox();
        var localCenter = bbox.Center;
        var localExtents = bbox.HalfExtents;

        // transform local center to World space
        var worldCenter = mtb.transform.TransformPoint(localCenter);

        // transform the local extents to World space
        var axisX = mtb.transform.TransformVector(localExtents.x, 0, 0);
        var axisY = mtb.transform.TransformVector(0, localExtents.y, 0);
        var axisZ = mtb.transform.TransformVector(0, 0, localExtents.z);

        Vector3 worldExtents = Vector3.zero;
        worldExtents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        worldExtents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        worldExtents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = worldCenter, extents = worldExtents };
    }

    bool IsModelTrackedInView(ModelTargetBehaviour modelTarget)
    {
        if (!modelTarget)
        {
            return false;
        }

        if (modelTarget.CurrentStatus == TrackableBehaviour.Status.NO_POSE)
        {
            return false;
        }

        var cam = DigitalEyewearARController.Instance.PrimaryCamera;
        if (!cam)
        {
            return false;
        }

        // Compute the center of the model in World coordinates
        Bounds modelBounds = GetModelTargetWorldBounds(modelTarget);

        var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, modelBounds);
    }

    #endregion PRIVATE_METHODS


    #region PUBLIC_METHODS

    public TargetFinder GetTargetFinder()
    {
        return this.targetFinder;
    }

    #endregion  // PUBLIC_METHODS


    #region BUTTON_METHODS

    // This method is called by UI Reset Button
    public void ResetModelReco(bool destroyGameObjects)
    {
        Debug.Log("<color=cyan>ResetModelReco() called.</color>");

        var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        var deviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        if (objectTracker != null)
        {
            objectTracker.Stop();

            if (this.targetFinder != null)
            {
                this.targetFinder.ClearTrackables(destroyGameObjects);
                this.targetFinder.Stop();
                this.targetFinder.StartRecognition();
            }
            else
            {
                Debug.LogError("Could not reset TargetFinder");
            }

            objectTracker.Start();
        }
        else
        {
            Debug.LogError("Could not reset ObjectTracker");
        }

        if (deviceTracker != null)
        {
            deviceTracker.Reset();
        }
        else
        {
            Debug.LogError("Could not reset DeviceTracker");
        }
    }

    // This method is called by UI Reset Button
    public void ResetGuideViews()
    {
        // Depending on tracking state, the guide view will be set to
        // "2D" or "No Guide View". We need to reset it to "2D" so that
        // a guide view will appear on reset
        ModelTargetBehaviour[] modelTargets = FindObjectsOfType<ModelTargetBehaviour>();

        foreach (var modelTarget in modelTargets)
        {
            modelTarget.GuideViewMode = ModelTargetBehaviour.GuideViewDisplayMode.GuideView2D;
            modelTarget.gameObject.SetActive(false);
        }

        // Destroy the GuideView objects
        GuideViewRenderingBehaviour[] guideViewRenderingBehaviours = FindObjectsOfType<GuideViewRenderingBehaviour>();

        foreach (var renderingBehavior in guideViewRenderingBehaviours)
        {
            Destroy(renderingBehavior.gameObject);
        }
    }

    #endregion // BUTTON_METHODS
}
