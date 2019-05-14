/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

using System.Collections.Generic;

public class MenuOptions : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    CameraSettings m_CameraSettings;
    TrackableSettings m_TrackableSettings;
    Toggle m_DeviceTrackerToggle, m_AutofocusToggle, m_FlashToggle;
    Canvas m_OptionsMenuCanvas;
    OptionsConfig m_OptionsConfig;
    #endregion //PRIVATE_MEMBERS

    public bool IsDisplayed { get; private set; }

    #region MONOBEHAVIOUR_METHODS
    protected virtual void Start()
    {
        m_CameraSettings = FindObjectOfType<CameraSettings>();
        m_TrackableSettings = FindObjectOfType<TrackableSettings>();
        m_OptionsConfig = FindObjectOfType<OptionsConfig>();
        m_OptionsMenuCanvas = GetComponentInChildren<Canvas>(true);
        m_DeviceTrackerToggle = FindUISelectableWithText<Toggle>("Tracker");
        m_AutofocusToggle = FindUISelectableWithText<Toggle>("Autofocus");
        m_FlashToggle = FindUISelectableWithText<Toggle>("Flash");

        var vuforia = VuforiaARController.Instance;
        vuforia.RegisterOnPauseCallback(OnPaused);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    public void ToggleAutofocus(bool enable)
    {
        if (m_CameraSettings)
            m_CameraSettings.SwitchAutofocus(enable);
    }

    public void ToggleTorch(bool enable)
    {
        if (m_FlashToggle && m_CameraSettings)
        {
            m_CameraSettings.SwitchFlashTorch(enable);

            // Update UI toggle status (ON/OFF) in case the flash switch failed
            m_FlashToggle.isOn = m_CameraSettings.IsFlashTorchEnabled();
        }
    }


    public void ToggleExtendedTracking(bool enable)
    {
        if (m_TrackableSettings)
            m_TrackableSettings.ToggleDeviceTracking(enable);
    }

    public void ActivateDataset(string datasetName)
    {
        if (m_TrackableSettings)
            m_TrackableSettings.ActivateDataSet(datasetName);
    }

    public void UpdateUI()
    {
        if (m_DeviceTrackerToggle && m_TrackableSettings)
            m_DeviceTrackerToggle.isOn = m_TrackableSettings.IsDeviceTrackingEnabled();

        if (m_FlashToggle && m_CameraSettings)
            m_FlashToggle.isOn = m_CameraSettings.IsFlashTorchEnabled();

        if (m_AutofocusToggle && m_CameraSettings)
            m_AutofocusToggle.isOn = m_CameraSettings.IsAutofocusEnabled();
    }

    public void ResetDeviceTracker()
    {
        var objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        if (objTracker != null && objTracker.IsActive)
        {
            Debug.Log("Stopping the ObjectTracker...");
            objTracker.Stop();

            List<DataSet> tempDataSetList = new List<DataSet>();

            // Create a temporary list of active datasets to prevent
            // InvalidOperationException caused by modifying the active
            // dataset list while iterating through it
            foreach (DataSet dataset in objTracker.GetDataSets())
            {
                tempDataSetList.Add(dataset);
            }

            // Reset active datasets
            foreach (DataSet dataset in tempDataSetList)
            {
                objTracker.DeactivateDataSet(dataset);
                objTracker.ActivateDataSet(dataset);
            }

            Debug.Log("Restarting the ObjectTracker...");
            objTracker.Start();
        }

        var deviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        if (deviceTracker != null && deviceTracker.Reset())
        {
            Debug.Log("Successfully reset device tracker");
        }
        else
        {
            Debug.LogError("Failed to reset device tracker");
        }
    }


    public void ShowOptionsMenu(bool show)
    {
        if (m_OptionsConfig && m_OptionsConfig.AnyOptionsEnabled())
        {
            if (show)
            {
                UpdateUI();
                m_OptionsMenuCanvas.gameObject.SetActive(true);
                m_OptionsMenuCanvas.enabled = true;
                IsDisplayed = true;
            }
            else
            {
                m_OptionsMenuCanvas.gameObject.SetActive(false);
                m_OptionsMenuCanvas.enabled = false;
                IsDisplayed = false;
            }
        }
    }

    public void CycleGuideView()
    {
        var modelTarget = FindObjectOfType<ModelTargetBehaviour>().ModelTarget;

        int guideViewIndexToActivate =
            (modelTarget.GetActiveGuideViewIndex() + 1) % modelTarget.GetNumGuideViews();
        modelTarget.SetActiveGuideViewIndex(guideViewIndexToActivate);
    }

    #endregion //PUBLIC_METHODS


    #region PROTECTED_METHODS
    protected T FindUISelectableWithText<T>(string text) where T : UnityEngine.UI.Selectable
    {
        T[] uiElements = GetComponentsInChildren<T>(true);
        foreach (var uielem in uiElements)
        {
            string childText = uielem.GetComponentInChildren<Text>().text;
            if (childText.Contains(text))
                return uielem;
        }
        return null;
    }
    #endregion //PROTECTED_METHODS

    #region PRIVATE_METHODS
    private void OnPaused(bool paused)
    {
        if (paused)
        {
            // Handle any tasks when app is paused here:
        }
        else
        {
            // Handle any tasks when app is resume here:

            // The flash torch is switched off by the OS automatically when app is paused.
            // On resume, update torch UI toggle to match torch status.
            UpdateUI();
        }
    }
    #endregion //PRIVATE_METHODS

}
