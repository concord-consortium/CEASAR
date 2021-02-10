using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
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
    void Start()
    {
        _renderer = GetComponent<Renderer>();
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
        }
    }
    public void Init(ConstellationsController controller, Star star, float maxMagnitude, float magnitudeScale, float radius)
    {
        constellationsController = controller;
        starData = star;
        transform.position = star.CalculateEquitorialPosition(radius);
        initialScale = transform.localScale;
        sceneInitialScale = transform.localScale;
        SetStarScale(magnitudeScale);
        starColor = StarColor.GetColorFromColorIndexSimple(star.ColorIndex);
        Utils.SetObjectColor(this.gameObject, starColor);
        transform.LookAt(constellationsController.transform);
        if (starHighlightOutline != null) starHighlightOutline.SetActive(false);
    }

    public void SetStarScale(float magnitudeScale)
    {
        var magScaleValue = DataManager.Instance.GetRelativeMagnitude(starData.Mag) * magnitudeScale;// ((starData.Mag * -1) + maxMagnitude + 1) * magnitudeScale;
        sceneInitialScale = initialScale * magScaleValue;
        transform.localScale = sceneInitialScale;
    }

    public void HandleSelectStar(bool broadcastToNetwork = false)
    {
        SimulationManager manager = SimulationManager.Instance;
        if (manager.NetworkControllerComponent.IsConnected)
        {
            HandleSelectStar(broadcastToNetwork, manager.LocalPlayerColor);
        }
        else
        {
            HandleSelectStar(broadcastToNetwork, starColor);
        }
    }

    public void HandleSelectStar(bool broadcastToNetwork, Color playerColor)
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
            
            SimulationEvents.Instance.StarSelected.Invoke(starData);
            if (broadcastToNetwork)
            {
                InteractionController interactionController = FindObjectOfType<InteractionController>();
                interactionController.ShowCelestialObjectInteraction(starData.ProperName,
                    starData.Constellation, starData.uniqueId, true);
            }
        }
    }
    public void CursorHighlightStar(bool highlight)
    {
        if (highlight)
        {
            transform.localScale = sceneInitialScale * scaleFactor;
        } else
        {
            transform.localScale = sceneInitialScale;
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
        Renderer rend = GetComponent<Renderer>();
        Collider coll = GetComponent<Collider>();
        rend.enabled = show;
        coll.enabled = show;
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleSelectStar(true);
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

}
