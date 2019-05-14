/*==============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/
using UnityEngine;
using Vuforia;

/// <summary>
///     A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class ModelTargetTrackableEventHandler : DefaultTrackableEventHandler
{
    #region PUBLIC_MEMBERS

    TrackedStateManager trackedStateManager;

    #endregion // PUBLIC_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Awake()
    {
        this.trackedStateManager = FindObjectOfType<TrackedStateManager>();
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region PROTECTED_METHODS

    protected override void OnTrackingFound()
    {
        Debug.Log("<color=green>OnTrackingFound() called: </color>" + mTrackableBehaviour.TrackableName);

        base.OnTrackingFound();

        this.trackedStateManager.TrackingFound(mTrackableBehaviour.TrackableName);
    }

    protected override void OnTrackingLost()
    {
        Debug.Log("<color=orange>OnTrackingLost() called: </color>" + mTrackableBehaviour.TrackableName);

        base.OnTrackingLost();

        if (m_PreviousStatus == TrackableBehaviour.Status.DETECTED ||
            m_PreviousStatus == TrackableBehaviour.Status.TRACKED ||
            m_PreviousStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            this.trackedStateManager.TrackingLost(mTrackableBehaviour.TrackableName);
        }
    }

    #endregion // PROTECTED_METHODS
}
