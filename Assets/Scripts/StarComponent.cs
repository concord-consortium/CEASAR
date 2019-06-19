using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private ConstellationsController constellationsController;
    private Vector3 initialScale;
    private float scaleFactor = 1.5f;
    private Vector3 currentScale;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        MainUIController mainUIController = FindObjectOfType<MainUIController>();
        if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
        if (mainUIController && mainUIController.starInfoPanel)
        {
            if (constellationsController) constellationsController.HighlightSingleConstellation(starData.ConstellationFullName);
            mainUIController.ChangeStarSelection(this.gameObject);
            mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
        }
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    initialScale = transform.localScale;
    //    transform.localScale = initialScale * scaleFactor;
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    transform.localScale = initialScale;
    //}
    public void OnPointerEnter(PointerEventData eventData)
    {
        currentScale = transform.localScale;
        transform.localScale = currentScale * scaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = currentScale;
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
}
