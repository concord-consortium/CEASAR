using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject StarVisible;

    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private ConstellationsController constellationsController;
    private MenuController menuController;
    private Vector3 initialScale;
    // Scene-specific value
    private Vector3 sceneInitialScale;
    private float scaleFactor = 2f;

    public GameObject starHighlightOutline;
    private SimulationManager manager {get {return SimulationManager.Instance;}}
    private Renderer _renderer;

    public GameObject FloatingInfoPanelPrefab;
    private GameObject FloatingInfoPanel;

    void Start()
    {
        _renderer = StarVisible.GetComponent<Renderer>();
        events.StarSelected.AddListener(StarSelected);
        events.MoonSelected.AddListener(MoonSelected);
        events.SunSelected.AddListener(SunSelected);
        events.PushPinSelected.AddListener(LocationUpdated);
        events.SimulationTimeChanged.AddListener(TimeUpdated);
    }
    void Update()
    {
        // sleep this if we're not visible
        if (_renderer.isVisible)
        {
            if (starHighlightOutline != null)
            {
                if (manager.CurrentlySelectedStar != null && manager.CurrentlySelectedStar == this)
                {
                    if (starHighlightOutline.activeInHierarchy == false) starHighlightOutline.SetActive(true);
                }
                else
                {
                    if (starHighlightOutline.activeInHierarchy)
                    {
                        starHighlightOutline.SetActive(false);
                    }
                }
            }
            // WTD only do this at load time, when we set location, when we set time, or move the sphere
            transform.LookAt(new Vector3(0,0,0));
        }
    }
    public void Init(ConstellationsController controller, Star star, float maxMagnitude, float magnitudeScale, float radius)
    {
        constellationsController = controller;
        starData = star;
        transform.position = star.CalculateEquitorialPosition(radius);
        initialScale = transform.localScale;
        sceneInitialScale = StarVisible.transform.localScale;
        SetStarScale(magnitudeScale);
        starColor = StarColor.GetColorFromColorIndexSimple(star.ColorIndex);
        Utils.SetObjectColor(this.StarVisible, starColor);
        transform.LookAt(constellationsController.transform);
        if (starHighlightOutline != null) starHighlightOutline.SetActive(false);
    }

    public void SetStarScale(float magnitudeScale)
    {
        var magScaleValue = DataManager.Instance.GetRelativeMagnitude(starData.Mag) * magnitudeScale;// ((starData.Mag * -1) + maxMagnitude + 1) * magnitudeScale;
        sceneInitialScale = initialScale * magScaleValue;
        StarVisible.transform.localScale = sceneInitialScale;
    }

    // this is called when another star is selected
    private void StarSelected(Star selectedStarData, string playerName, Color playerColor)
    {
        if (selectedStarData.Hipparcos != starData.Hipparcos && FloatingInfoPanel)
        {
            if (FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerName == playerName)
                Destroy(FloatingInfoPanel);
        }
    }
    private void SunSelected(bool selected)
    {
        if (FloatingInfoPanel)
            Destroy(FloatingInfoPanel);
    }
    private void MoonSelected(bool selected)
    {
        if (FloatingInfoPanel)
        {
            // FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerName !=
            Destroy(FloatingInfoPanel);
        }
    }

    public void HandleSelectStar(bool broadcastToNetwork = false)
    {
        SimulationManager manager = SimulationManager.Instance;
        if (manager.NetworkControllerComponent.IsConnected)
        {
            HandleSelectStar(broadcastToNetwork, manager.LocalUsername, manager.LocalPlayerColor);
        }
        else
        {
            HandleSelectStar(broadcastToNetwork, "ceasar_user", starColor);
        }
    }

    public void HandleSelectStar(bool broadcastToNetwork, string playerName, Color playerColor)
    {
        if (!menuController) menuController = FindObjectOfType<MenuController>();
        if (!menuController.IsDrawing)
        {
            CCDebug.Log("Selected star: " + starData.uniqueId, LogLevel.Info, LogMessageCategory.Interaction);
            if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();

            if (constellationsController)
            {
                constellationsController.HighlightSingleConstellation(starData.ConstellationFullName, playerColor);
            }

            // make sure it's visible
            SimulationManager.Instance.CurrentlySelectedStar = this;
            SimulationManager.Instance.CurrentlySelectedConstellation = this.starData.ConstellationFullName;

            SimulationEvents.Instance.StarSelected.Invoke(starData, playerName, playerColor);
            if (broadcastToNetwork)
            {
                InteractionController interactionController = FindObjectOfType<InteractionController>();
                interactionController.ShowCelestialObjectInteraction(starData.ProperName,
                    starData.Constellation, starData.uniqueId, true);
            }

            // make a new floating info panel that is a child of the star
            FloatingInfoPanel = Instantiate(FloatingInfoPanelPrefab, new Vector3(0, -8f, 6f), new Quaternion(0, 0, 0, 0), this.transform);
            FloatingInfoPanel.transform.localPosition = new Vector3(0, -6f, 5f);
            FloatingInfoPanel.transform.localScale = new Vector3(.8f, .8f, .8f);
            FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerName = playerName;
            FloatingInfoPanel.GetComponent<FloatingInfoPanel>().playerColor = playerColor;

            setStarSelectedText();
        }
    }

    private void setStarSelectedText()
    {
        StringBuilder description = new StringBuilder();
        if (starData == null)
        {
            description.Append("Now showing constellation: ");
            description.Append(manager.CurrentlySelectedConstellation);
        }
        else
        {
            double longitudeTimeOffset = manager.CurrentLatLng.Longitude/15d;
            double lst = manager.CurrentSimulationTime.ToSiderealTime() + longitudeTimeOffset;
            AltAz altAz = Utils.CalculateAltitudeAzimuthForStar(starData.RA, starData.Dec,
                lst, manager.CurrentLatLng.Latitude);
            double ra = starData.RA;
            description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
            description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
            description.Append("Alt/Az: ")
                .Append(altAz.Altitude.ToString("F2"))
                .Append("°, ")
                .Append(altAz.Azimuth.ToString("F2"))
                .Append("°  Mag: ")
                .AppendLine(starData.Mag.ToString());
            description.Append("R.A.: ")
                .Append(ra.ToString("F2"))
                .Append("h  Dec: ")
                .Append(starData.Dec.ToString("F2"))
                .Append("°")
                // A value of 10000000 indicates missing or dubious (e.g., negative) parallax data in Hipparcos
                .Append(starData.Dist != 10000000 ?  "  Dist: " + (starData.Dist * 3.262f).ToString("F0") + " ly": "");
        }
        FloatingInfoPanel.GetComponent<FloatingInfoPanel>().InfoText.SetText(description.ToString());
    }

    private void LocationUpdated(Pushpin pin)
    {
        if (FloatingInfoPanel)
            setStarSelectedText();
    }
    private void TimeUpdated()
    {
        if (FloatingInfoPanel)
            setStarSelectedText();
    }

    public void CursorHighlightStar(bool highlight)
    {
        if (highlight)
        {
            StarVisible.transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            StarVisible.transform.localScale = sceneInitialScale;
        }
        // Track if we're hovering over a star
        constellationsController.HoveredStar = highlight ? this : null;
    }

    public void SetStarColor(Color constellationColor, Color starColor)
    {
        // Store star color - not yet using real values
        this.starColor = starColor;
        // Store constellation color
        this.constellationColor = constellationColor;
    }

    public void ShowStar(bool show)
    {
        Renderer rend = StarVisible.GetComponent<Renderer>();
        Collider coll = GetComponent<Collider>();
        rend.enabled = show;
        coll.enabled = show;
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
            HandleSelectStar(true);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorHighlightStar(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorHighlightStar(false);
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
