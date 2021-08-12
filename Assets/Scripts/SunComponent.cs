﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using SunCalcNet;

public class SunComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject SunVisible;

    private MenuController menuController;
    // Scene-specific value
    private Vector3 sceneInitialScale;
    private float scaleFactor = 1.1f;

    public GameObject FloatingInfoPanelPrefab;
    private GameObject FloatingInfoPanel;

    // Start is called before the first frame update
    void Start()
    {
        events.StarSelected.AddListener(StarSelected);
        events.SunSelected.AddListener(SunSelected);
        events.MoonSelected.AddListener(MoonSelected);
        events.PushPinSelected.AddListener(LocationUpdated);
        events.SimulationTimeChanged.AddListener(TimeUpdated);
        sceneInitialScale = SunVisible.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(0,0,0));
    }

    public void CursorHighlightSun(bool highlight)
    {
        if (highlight)
        {
            SunVisible.transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            SunVisible.transform.localScale = sceneInitialScale;
        }
    }

    // this is called when another object is selected
    private void StarSelected(Star selectedStarData)
    {
        Destroy(FloatingInfoPanel);
    }
    private void SunSelected(bool selected)
    {
        Destroy(FloatingInfoPanel);
    }
    private void MoonSelected(bool selected)
    {
        Destroy(FloatingInfoPanel);
    }

    public void HandleSelectSun(bool broadcastToNetwork = false)
    {
        if (!menuController) menuController = FindObjectOfType<MenuController>();
        if (!menuController.IsDrawing)
        {
            CCDebug.Log("Selected sun", LogLevel.Info, LogMessageCategory.Interaction);
            SimulationEvents.Instance.SunSelected.Invoke(true);

            // make a new floating info panel that is a child of the object
            FloatingInfoPanel = Instantiate(FloatingInfoPanelPrefab, new Vector3(0, -8f, 6f), new Quaternion(0, 0, 0, 0), this.transform);
            FloatingInfoPanel.transform.localPosition = new Vector3(0, -10f, 6f);

            setSunSelectedText();
        }
    }

    private void setSunSelectedText()
    {
        var solarPosition = SunCalc.GetSunPosition(manager.CurrentSimulationTime, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);
        double azimuth = 180f + (solarPosition.Azimuth * 180f / Mathf.PI);
        double altitude = solarPosition.Altitude * 180f / Mathf.PI;
        StringBuilder description = new StringBuilder();
        description.AppendLine("The Sun");
        description.Append("Alt/Az: ")
            .Append(altitude.ToString("F2"))
            .Append(", ")
            .AppendLine(azimuth.ToString("F2"));
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().InfoText.SetText(description.ToString());
    }

    private void LocationUpdated(Pushpin pin)
    {
        if (FloatingInfoPanel)
            setSunSelectedText();
    }
    private void TimeUpdated()
    {
        if (FloatingInfoPanel)
            setSunSelectedText();
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        if (FloatingInfoPanel)
        {
            Destroy(FloatingInfoPanel);
        }
        else
        {
             HandleSelectSun(true);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorHighlightSun(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorHighlightSun(false);
    }
    #endregion

    private void removeAllListeners()
    {
        events.StarSelected.RemoveListener(StarSelected);
        events.MoonSelected.RemoveListener(MoonSelected);
        events.SunSelected.RemoveListener(SunSelected);
        events.PushPinSelected.RemoveListener(LocationUpdated);
        events.SimulationTimeChanged.RemoveListener(TimeUpdated);
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }
}
