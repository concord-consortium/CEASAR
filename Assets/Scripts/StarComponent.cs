using UnityEngine;
using UnityEngine.EventSystems;

public class StarComponent : MonoBehaviour
{
    public Star starData;
    public Color starColor;
    public Color constellationColor;

    private MainUIController mainUIController;
    private ConstellationsController constellationsController;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
        constellationsController = FindObjectOfType<ConstellationsController>();
    }

    void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject()
            && mainUIController && mainUIController.starInfoPanel)
        {
            if (constellationsController) constellationsController.HighlightSingleConstellation(starData.ConstellationFullName);
            mainUIController.ChangeStarSelection(this.gameObject);
            mainUIController.ChangeConstellationHighlight(starData.ConstellationFullName);
        }
    }

    public void SetStarColor(Color constellationColor, Color starColor)
    {
        // Store star color - not yet using real values
        this.starColor = starColor;
        // Store constellation color
        this.constellationColor = constellationColor;

    }
}
