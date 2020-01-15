using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private ConstellationsController constellationsController;
    private Vector3 initialScale;
    private float scaleFactor = 2f;

    public void Init(ConstellationsController controller, Star star, float maxMagnitude, float magnitudeScale, float radius)
    {
        constellationsController = controller;
        starData = star;
        transform.position = star.CalculateEquitorialPosition(radius);
        initialScale = transform.localScale;
        SetStarScale(maxMagnitude, magnitudeScale);
        transform.LookAt(constellationsController.transform);
    }

    public void SetStarScale(float maxMagnitude, float magnitudeScale)
    {
        var magScaleValue = SimulationManager.GetInstance().GetRelativeMagnitude(starData.Mag) * magnitudeScale;// ((starData.Mag * -1) + maxMagnitude + 1) * magnitudeScale;
        Vector3 magScale = initialScale * magScaleValue;
        transform.localScale = magScale;
    }

    public void HandleSelectStar(bool broadcastToNetwork = false)
    {
        Debug.Log("Selected star: " + starData.uniqueId);
        MainUIController mainUIController = FindObjectOfType<MainUIController>();
        if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
        if (mainUIController && mainUIController.starInfoPanel)
        {
            if (constellationsController) constellationsController.HighlightSingleConstellation(starData.ConstellationFullName);
            // make sure it's visible
            mainUIController.ShowPanel("StarInfoPanel");

            SimulationManager.GetInstance().CurrentlySelectedStar = this;
            FindObjectOfType<StarInfoPanel>().UpdateStarInfoPanel();
            
            // update dropdown, if visible
            mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
        }

        if (broadcastToNetwork)
        {
            NetworkController network = FindObjectOfType<NetworkController>();
            network.ShowCelestialObjectInteraction(starData.ProperName,
                starData.Constellation, starData.uniqueId, true);
        }
    }
    public void CursorHighlightStar(bool highlight)
    {
        if (highlight)
        {
            // currentScale = transform.localScale;
            transform.localScale = initialScale * scaleFactor;
        } else
        {
            transform.localScale = initialScale;
        }
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
