/*============================================================================== 
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/
using UnityEngine;
using UnityEngine.UI;

public class TrackedStateManager : MonoBehaviour
{
    #region PUBLIC_MEMBERS

    public CanvasGroup introPanel;
    public CanvasGroup targetStatusPanel;
    public CanvasGroup resetButton;
    public Image reticle;
    public Image landerStatusImage;
    public Image bikeStatusImage;

    #endregion  // PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS

    Target bikeTarget;
    Target landerTarget;

    struct Target
    {
        public TrackedState state;
        public Image statusImage;
        public Sprite[] statusSprites;
    }

    enum TrackedState
    {
        Passive,
        Recognized,
        Snapped
    };

    enum UIState
    {
        Intro,
        Idle,
        Active
    };

    #endregion // PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    void Awake()
    {
        this.bikeTarget.state = TrackedState.Passive;
        this.bikeTarget.statusImage = this.bikeStatusImage;
        this.bikeTarget.statusSprites = new Sprite[]
        {
            Resources.Load<Sprite>("BikeStatusPassive"),
            Resources.Load<Sprite>("BikeStatusRecognized"),
            Resources.Load<Sprite>("BikeStatusSnapped")
        };

        this.landerTarget.state = TrackedState.Passive;
        this.landerTarget.statusImage = this.landerStatusImage;
        this.landerTarget.statusSprites = new Sprite[]
        {
            Resources.Load<Sprite>("LanderStatusPassive"),
            Resources.Load<Sprite>("LanderStatusRecognized"),
            Resources.Load<Sprite>("LanderStatusSnapped")
        };
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region VUFORIA_CALLBACK_HANDLERS

    // Called by ModelRecoEventHandler.OnNewSearchResult()
    public void TargetFinderFound(string targetName)
    {
        UpdateStatus(targetName, TrackedState.Recognized);

        SetUIState(UIState.Active);
    }

    // Called by ModelTargetTrackableEventHandler.OnTrackingFound()
    public void TrackingFound(string targetName)
    {
        UpdateStatus(targetName, TrackedState.Snapped);
    }

    // Called by ModelTargetTrackableEventHandler.OnTrackingLost()
    public void TrackingLost(string targetName)
    {
        UpdateStatus(targetName, TrackedState.Recognized);
    }

    #endregion  // VUFORIA_CALLBACK_HANDLERS


    #region PRIVATE_METHODS

    void UpdateStatus(string targetName, TrackedState state)
    {
        if (targetName.Equals("Vuforia_MarsLander"))
        {
            ToggleStatusImage(ref this.landerTarget, state);
        }
        else if (targetName.Equals("Vuforia_Motorcycle"))
        {
            ToggleStatusImage(ref this.bikeTarget, state);
        }
    }

    void ToggleStatusImage(ref Target target, TrackedState state)
    {
        target.statusImage.sprite = target.statusSprites[(int)state];
        target.state = state;
    }

    void ResetTrackedState(ref Target target)
    {
        target.statusImage.sprite = target.statusSprites[(int)TrackedState.Passive];
        target.state = TrackedState.Passive;
    }

    void SetUIState(UIState uiState)
    {
        // Check to see if any of the UI elements are null before we use them below
        if (!(this.introPanel && this.targetStatusPanel && this.reticle && this.resetButton))
        {
            string error =
                "One or more UI element references for this script in the Inspector is null. " +
                "Please check your scene.";

            Debug.LogError(error);
            return;
        }

        // Intro State
        this.introPanel.alpha = (uiState == UIState.Intro) ? 1 : 0;
        this.introPanel.interactable = (uiState == UIState.Intro);
        // Active & Idle State
        this.targetStatusPanel.alpha = (uiState == UIState.Intro) ? 0 : 1;
        this.resetButton.alpha = (uiState == UIState.Intro) ? 0 : 1;
        this.resetButton.interactable = (uiState != UIState.Intro);
        // Idle State Only
        this.reticle.enabled = (uiState == UIState.Idle);
    }

    #endregion // PRIVATE_METHODS


    #region BUTTON_METHODS

    public void GetStartedButton()
    {
        SetUIState(UIState.Idle);
    }

    public void ResetTutorial()
    {
        ResetTrackedState(ref bikeTarget);
        ResetTrackedState(ref landerTarget);

        SetUIState(UIState.Intro);
    }

    #endregion // BUTTON_METHODS
}
