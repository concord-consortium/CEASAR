using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private ConstellationsController constellationsController;
    private MainUIController mainUIController;
    private Vector3 initialScale;
    // Scene-specific value
    private Vector3 sceneInitialScale;
    private float scaleFactor = 2f;

    public void Init(ConstellationsController controller, Star star, float maxMagnitude, float magnitudeScale, float radius)
    {
        constellationsController = controller;
        starData = star;
        transform.position = star.CalculateEquitorialPosition(radius);
        initialScale = transform.localScale;
        sceneInitialScale = transform.localScale;
        SetStarScale(maxMagnitude, magnitudeScale);
        transform.LookAt(constellationsController.transform);
    }

    public void SetStarScale(float maxMagnitude, float magnitudeScale)
    {
        var magScaleValue = SimulationManager.GetInstance().GetRelativeMagnitude(starData.Mag) * magnitudeScale;// ((starData.Mag * -1) + maxMagnitude + 1) * magnitudeScale;
        sceneInitialScale = initialScale * magScaleValue;
        transform.localScale = sceneInitialScale;
    }

    public void HandleSelectStar(bool broadcastToNetwork = false)
    {
        SimulationManager manager = SimulationManager.GetInstance();
        if (manager.NetworkControllerComponent.IsConnected)
        {
            HandleSelectStar(broadcastToNetwork, manager.LocalPlayerColor);
        }
        else
        {
            HandleSelectStar(broadcastToNetwork, Color.white);
        }
    }
    public void HandleSelectStar(bool broadcastToNetwork, Color playerColor)
    {
        if (!mainUIController) mainUIController = FindObjectOfType<MainUIController>();
        if (!mainUIController.IsDrawing)
        {
            Debug.Log("Selected star: " + starData.uniqueId);
            MainUIController mainUIController = FindObjectOfType<MainUIController>();
            if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
            if (mainUIController && mainUIController.starInfoPanel)
            {
                if (constellationsController)
                {
                    constellationsController.HighlightSingleConstellation(starData.ConstellationFullName, playerColor);
                }

                // make sure it's visible
                SimulationManager.GetInstance().CurrentlySelectedStar = this;
                mainUIController.ShowPanel("StarInfoPanel");

                mainUIController.starInfoPanel.GetComponent<StarInfoPanel>().UpdateStarInfoPanel();

                // update dropdown, if visible
                mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
            }

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
