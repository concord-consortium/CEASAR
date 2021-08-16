using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using SunCalcNet;

public class MoonComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject MoonVisible;

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
        events.MoonSelected.AddListener(MoonSelected);
        events.SunSelected.AddListener(SunSelected);
        events.PushPinSelected.AddListener(LocationUpdated);
        events.SimulationTimeChanged.AddListener(TimeUpdated);
        sceneInitialScale = MoonVisible.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(0,0,0));
    }

    // this is called when another object is selected
    private void StarSelected(Star selectedStarData, string playerName, Color playerColor)
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

    public void HandleSelectMoon(bool broadcastToNetwork = false)
    {
        if (!menuController) menuController = FindObjectOfType<MenuController>();
        if (!menuController.IsDrawing)
        {
            CCDebug.Log("Selected moon", LogLevel.Info, LogMessageCategory.Interaction);
            SimulationEvents.Instance.MoonSelected.Invoke(true);

            // make a new floating info panel that is a child of the object
            FloatingInfoPanel = Instantiate(FloatingInfoPanelPrefab, new Vector3(0, -8f, 6f), new Quaternion(0, 0, 0, 0), this.transform);
            FloatingInfoPanel.transform.localPosition = new Vector3(0, -9f, 5f);
            FloatingInfoPanel.transform.localScale = new Vector3(.8f, .8f, .8f);

            setMoonSelectedText();
        }
    }

    private void setMoonSelectedText()
    {
        var lunarPosition = MoonCalc.GetMoonPosition(manager.CurrentSimulationTime, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);
        double azimuth = 180f + (lunarPosition.Azimuth * 180f / Mathf.PI);
        double altitude = lunarPosition.Altitude * 180f / Mathf.PI;
        double declination = lunarPosition.Declination * 180f / Mathf.PI;
        double ra = lunarPosition.RA;
        double raInDegrees = ra * 180f / Mathf.PI;
        double raInHours = raInDegrees / 15f;
        if (raInHours < 0) raInHours = 24f + raInHours;
        StringBuilder description = new StringBuilder();
        description.AppendLine("The Moon");
        description.Append("Alt/Az: ")
            .Append(altitude.ToString("F2"))
            .Append("°, ")
            .Append(azimuth.ToString("F2"))
            .AppendLine("°");
        description.Append("R.A.: ")
            .Append(raInHours.ToString("F2"))
            .Append("h  Dec: ")
            .Append(declination.ToString("F2"))
            .AppendLine("°");
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().InfoText.SetText(description.ToString());
    }

    private void LocationUpdated(Pushpin pin)
    {
        if (FloatingInfoPanel)
            setMoonSelectedText();
    }
    private void TimeUpdated()
    {
        if (FloatingInfoPanel)
            setMoonSelectedText();
    }

    public void CursorHighlightMoon(bool highlight)
    {
        if (highlight)
        {
            MoonVisible.transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            MoonVisible.transform.localScale = sceneInitialScale;
        }
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
            HandleSelectMoon(true);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorHighlightMoon(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorHighlightMoon(false);
    }
    #endregion

    private void removeAllListeners()
    {
        events.StarSelected.RemoveListener(StarSelected);
        events.SunSelected.RemoveListener(SunSelected);
        events.MoonSelected.RemoveListener(MoonSelected);
        events.PushPinSelected.RemoveListener(LocationUpdated);
        events.SimulationTimeChanged.RemoveListener(TimeUpdated);
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }
}
