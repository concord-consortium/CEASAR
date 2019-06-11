using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private MainUIController mainUIController;
    private ConstellationsController constellationsController;
    private Vector3 initialScale;
    private float scaleFactor = 1.5f;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
        constellationsController = FindObjectOfType<ConstellationsController>();
        initialScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (mainUIController && mainUIController.starInfoPanel)
        {
            if (constellationsController) constellationsController.HighlightSingleConstellation(starData.ConstellationFullName);
            mainUIController.ChangeStarSelection(this.gameObject);
            mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = initialScale * scaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = initialScale;
    }

    public void SetStarColor(Color constellationColor, Color starColor)
    {
        // Store star color - not yet using real values
        this.starColor = starColor;
        // Store constellation color
        this.constellationColor = constellationColor;

    }
}
